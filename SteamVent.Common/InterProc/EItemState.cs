using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Common.InterProc
{
    [Flags]
    public enum EItemState : UInt32
    {
	    k_EItemStateNone            = 0x00, // item not tracked on client
	    k_EItemStateSubscribed      = 0x01, // current user is subscribed to this item. Not just cached.
	    k_EItemStateLegacyItem      = 0x02, // item was created with ISteamRemoteStorage
	    k_EItemStateInstalled       = 0x04, // item is installed and usable (but maybe out of date)
	    k_EItemStateNeedsUpdate     = 0x08, // items needs an update. Either because it's not installed yet or creator updated content
	    k_EItemStateDownloading     = 0x10, // item update is currently downloading
	    k_EItemStateDownloadPending = 0x20, // DownloadItem() was called for this item, content isn't available until DownloadItemResult_t is fired
	    k_EItemStateDisabledLocally = 0x40, // Item is disabled locally, so it shouldn't be considered subscribed
    };
}
