#requires -Version 5.1
# ===========================================================================
# SteamVent Steamworks vtable auditor  (signature-aware)
#
# Convention (game / consumer build -- the DLL is imported, not built):
#   *  #ifdef _PS3 ... #endif            -> EXCLUDED  (not in the Windows vtable)
#   *  STEAM_PRIVATE_API( virtual ... )  -> INCLUDED  (protected member, occupies a slot)
#   *  #pragma / STEAM_CALL_RESULT / STEAM_FLAT_NAME lines are NOT virtuals (skipped)
#
# Matching model (fixes the old name-based false positives):
#   *  Slot-by-slot, comparing a PARAMETER SIGNATURE = (param count | ordered param names).
#      This disambiguates C++ overloads (e.g. CreateQueryAllUGCRequest unPage/cursor;
#      GetQueryUGCKeyValueTag by-index/by-key) even though our C# names differ.
#   *  OK= same slot+count+names ; SOFT= same count, spelling differs ; DIFF= count differs (bug)
#   *  ref-only= ref exposes a slot we omit ; our-only= we expose a slot the ref lacks
# ===========================================================================
$ErrorActionPreference = 'Stop'
$root = 'F:\Programming\Battlezone-Redux-Mod-Manager\Deps\SteamVent\references'
$raw  = Join-Path $root '_svraw'
$gb   = Join-Path $root 'goldberg_emulator\sdk_includes'
$sw   = Join-Path $root 'steamworks_sdk'
$intf = 'F:\Programming\Battlezone-Redux-Mod-Manager\Deps\SteamVent\SteamVent\Steamworks\Native\Interfaces'
$script:auditResults  = New-Object System.Collections.ArrayList
$script:officialCache = @{}

# --- Parameter signature: "count|name1,name2,..." (overload disambiguator) ---
# Strips = default values, [attributes] (e.g. [In, Out] / [MarshalAs(..)]), and [] arrays
# from the WHOLE param list FIRST (so a comma inside an attribute does not split it),
# then takes each param's last identifier as its name.
function Get-ParamSig([string]$inner) {
    $inner = ($inner -replace '=.*$', '').Trim()
    if ($inner -eq '') { return '0|' }
    $inner = ($inner -replace '\[[^\]]*\]', '' -replace '\[\s*\]', '').Trim()
    $parts = @($inner -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' })
    $names = foreach ($p in $parts) {
        $ids = [regex]::Matches($p, '[A-Za-z_]\w*')
        if ($ids.Count -gt 0) { $ids[$ids.Count - 1].Value } else { '-' }
    }
    return ('{0}|{1}' -f $names.Count, ($names -join ','))
}

# --- Parse a C++ reference header -> ordered list of {idx,name,sig} ---
# Uses BALANCED-PAREN extraction so annotations like STEAM_OUT_STRING_COUNT( x ) inside
# the parameter list do not truncate the captured param list.
function Get-RefVtable([string]$file) {
    if (-not $file -or -not (Test-Path $file)) { return $null }
    $list = New-Object System.Collections.ArrayList
    $i = -1; $skip = 0
    foreach ($l in (Get-Content $file)) {
        $tr = $l.Trim()
        if ($skip -eq 0 -and ($tr -match '^#\s*if\s+defined\s*\(\s*_PS3\b' -or $tr -match '^#\s*ifdef\s+_PS3\b')) { $skip = 1; continue }
        if ($skip -gt 0) {
            if ($tr -match '^#\s*if\b') { $skip++ } elseif ($tr -match '^#\s*endif\b') { $skip-- }
            continue
        }
        if ($tr -notmatch 'virtual\b') { continue }
        # method name = identifier right before "(" that is not STEAM_PRIVATE_API / virtual
        $nm = [regex]::Match($tr, '([A-Za-z_]\w*)\s*\((?!\s*virtual)')
        if (-not $nm.Success) { continue }
        $name = $nm.Groups[1].Value
        $openIdx = $nm.Index + $nm.Length - 1          # index of the "("
        $depth = 0; $closeIdx = -1
        for ($p = $openIdx; $p -lt $tr.Length; $p++) {
            $c = $tr[$p]
            if ($c -eq '(') { $depth++ } elseif ($c -eq ')') { $depth--; if ($depth -eq 0) { $closeIdx = $p; break } }
        }
        if ($closeIdx -lt 0) { continue }
        $params = $tr.Substring($openIdx + 1, $closeIdx - $openIdx - 1)
        $i++
        [void]$list.Add([pscustomobject]@{ idx = $i; name = $name; sig = (Get-ParamSig $params) })
    }
    return $list
}

# --- Parse our C# file -> ordered list of {idx,name,sig} (delegates only) ---
function Get-OurVtable([string]$file) {
    if (-not (Test-Path $file)) { return $null }
    $h = Get-Content $file -Raw
    $list = New-Object System.Collections.ArrayList
    $rx = [regex]'\[VTableIndex\((\d+)\)[^\r\n]*\r?\n[^\r\n]*?delegate\s+[^;]*?(\w+?)Delegate\s*\(\s*IntPtr\s+thisPtr\s*,?\s*(.*?)\)\s*;'
    foreach ($m in $rx.Matches($h)) {
        [void]$list.Add([pscustomobject]@{ idx = [int]$m.Groups[1].Value; name = $m.Groups[2].Value; sig = (Get-ParamSig $m.Groups[3].Value) })
    }
    return $list
}

# --- Official Steamworks SDK: per-version -> tag whose isteamXXX.h carries that exact interface version ---
# Sourced from the references/steamworks_sdk git tags. The official repo does
# NOT tag ISteamClient013/014 or ISteamUGC004/005/006 (tags jump 012->015 and 003->007), so those fall
# back to the Goldberg Emulator SDK -- the SOLE source for those five snapshots.
$script:officialTag = @{
    'ISteamApps001'='1.00'; 'ISteamApps002'='1.01'; 'ISteamApps003'='1.04'; 'ISteamApps004'='1.12'
    'ISteamApps005'='1.18'; 'ISteamApps006'='1.27'; 'ISteamApps007'='1.32'; 'ISteamApps008'='1.37'
    'ISteamClient007'='1.00'; 'ISteamClient008'='1.04'; 'ISteamClient009'='1.08'; 'ISteamClient010'='1.12'
    'ISteamClient011'='1.16'; 'ISteamClient012'='1.17'; 'ISteamClient015'='1.30'; 'ISteamClient016'='1.31'
    'ISteamClient017'='1.32'; 'ISteamClient018'='1.43'; 'ISteamClient019'='1.46'
    'ISteamUGC001'='1.27'; 'ISteamUGC002'='1.30'; 'ISteamUGC003'='1.31'
    'ISteamUGC007'='1.34'; 'ISteamUGC008'='1.37'; 'ISteamUGC009'='1.39'; 'ISteamUGC010'='1.40'
    'ISteamUGC012'='1.43'; 'ISteamUGC013'='1.45'; 'ISteamUGC014'='1.47'; 'ISteamUGC015'='1.51'
    'ISteamUGC016'='1.54'
    'ISteamUtils007'='1.32'
}

# Extract the official SDK header for an interface into a temp file; $null when the repo lacks that version.
function Get-OfficialRef([string]$ifaceName) {
    if ($script:officialCache.ContainsKey($ifaceName)) { return $script:officialCache[$ifaceName] }
    $tag = $script:officialTag[$ifaceName]
    if (-not $tag) { return $null }
    $file = ('isteam' + ((($ifaceName -replace '^ISteam', '') -replace '\d{3}$', '').ToLower()) + '.h')
    $content = git -C $sw show ('{0}:sdk/public/steam/{1}' -f $tag, $file) 2>$null
    if (-not $content) { return $null }
    $tmpDir = Join-Path $root '_official'
    if (-not (Test-Path $tmpDir)) { New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null }
    $tmp = Join-Path $tmpDir ($ifaceName + '.h')
    ($content -join "`n") | Set-Content -LiteralPath $tmp -NoNewline
    $script:officialCache[$ifaceName] = $tmp
    return $tmp
}

# --- Resolve a reference header path: official Steamworks SDK first, Goldberg Emulator SDK as sole-source fallback ---
function Get-RefPath([string]$ourName) {
    $base = ($ourName -replace '\.cs$', '')
    $off = Get-OfficialRef $base
    if ($off) { return $off }
    $gbPath = Join-Path $gb (('isteam' + (($base -replace '^ISteam', '').ToLower())) + '.h')
    if (Test-Path $gbPath) { return $gbPath }
    return $null
}

function CompareVTable([string]$label, [string]$ourFile, [string]$refFile) {
    $res = [pscustomobject]@{ label = $label; ok = 0; soft = 0; diff = 0; refOnly = 0; ourOnly = 0; bad = @() }
    $our = Get-OurVtable $ourFile; $ref = Get-RefVtable $refFile
    Write-Output ('=' * 82)
    Write-Output $label
    if (-not $ref) { Write-Output '  (no reference file - SKIPPED)'; $res.bad += 'no-ref'; [void]$script:auditResults.Add($res); return }
    if (-not $our) { Write-Output '  (no our file - SKIPPED)';   $res.bad += 'no-ours'; [void]$script:auditResults.Add($res); return }

    $refMap = @{}; foreach ($r in $ref) { $refMap[$r.idx] = $r }
    $ourMap = @{}; foreach ($o in $our) { $ourMap[$o.idx] = $o }
    $maxIdx = 0
    foreach ($r in $ref) { if ($r.idx -gt $maxIdx) { $maxIdx = $r.idx } }
    foreach ($o in $our) { if ($o.idx -gt $maxIdx) { $maxIdx = $o.idx } }

    $refOnly = @(); $ourOnly = @()
    for ($n = 0; $n -le $maxIdx; $n++) {
        $r = $null; if ($refMap.ContainsKey($n)) { $r = $refMap[$n] }
        $o = $null; if ($ourMap.ContainsKey($n)) { $o = $ourMap[$n] }
        if ($r -and $o) {
            $rc = [int](($r.sig -split '\|')[0]); $oc = [int](($o.sig -split '\|')[0])
            if ($rc -eq $oc -and $r.sig -eq $o.sig) { $res.ok++ }
            elseif ($rc -eq $oc) {
                $res.soft++
                Write-Output ('  SOFT  {0,3}  ours={1} [{2}]   ref={3} [{4}]' -f $n, $o.name, $o.sig, $r.name, $r.sig)
            } else {
                $res.diff++
                $d = ('  ***   {0,3}  ours={1} [{2}]   ref={3} [{4}]' -f $n, $o.name, $o.sig, $r.name, $r.sig)
                $res.bad += $d.Trim(); Write-Output $d
            }
        } elseif ($r) { $res.refOnly++; $refOnly += ('{0}@{1}' -f $r.name, $n) }
        elseif ($o) { $res.ourOnly++; $ourOnly += ('{0}@{1}' -f $o.name, $n); $res.bad += ('our-only slot {0} {1}' -f $n, $o.name) }
    }
    Write-Output ('  => OK={0}  SOFT={1}  DIFF={2}   refOnly(omitted)={3}   ourOnly(extra)={4}' -f $res.ok, $res.soft, $res.diff, $res.refOnly, $res.ourOnly)
    if ($refOnly.Count -gt 0) { Write-Output ('  ref-only: ' + ($refOnly -join ', ')) }
    if ($ourOnly.Count -gt 0) { Write-Output ('  our-only: ' + ($ourOnly -join ', ')) }
    [void]$script:auditResults.Add($res)
}

# ============================= RUN THE AUDIT =============================
$files = Get-ChildItem $intf -Filter 'ISteam*.cs' | Where-Object { $_.Name -match '^ISteam[A-Za-z]+\d{3}\.cs$' } | Sort-Object Name
foreach ($f in $files) {
    $ref = Get-RefPath $f.Name
    $refLeaf = '(none)'
    if ($ref) { $refLeaf = (Split-Path $ref -Leaf) }
    CompareVTable ($f.Name + '   vs   ' + $refLeaf) $f.FullName $ref
}

Write-Output ''
Write-Output ('#' * 82)
Write-Output 'SUMMARY   (DIFF = arity mismatch [real bug]; SOFT = same arity, spelling differs; our-only = we expose a slot the ref lacks)'
foreach ($r in $script:auditResults) {
    $name = ($r.label -split '\s+')[0]
    $flag = ' ok  '
    if ($r.diff -gt 0) { $flag = ' *** ' } elseif ($r.ourOnly -gt 0) { $flag = ' WARN' }
    Write-Output ('{0}  {1,-19} OK={2,-3} SOFT={3,-3} DIFF={4,-2} refOnly={5,-3} ourOnly={6}' -f $flag, $name, $r.ok, $r.soft, $r.diff, $r.refOnly, $r.ourOnly)
}
$badCount = 0; foreach ($r in $script:auditResults) { if ($r.diff -gt 0) { $badCount++ } }
Write-Output ''
if ($badCount -eq 0) { Write-Output 'RESULT: NO arity mismatches (DIFF) detected across any interface.' }
else { Write-Output ('RESULT: {0} interface(s) report a DIFF - review the *** lines above.' -f $badCount) }