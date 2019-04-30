﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamClient.Interfaces
{
    public enum EAccountType : int
    {
        k_EAccountTypeInvalid = 0,
        k_EAccountTypeIndividual = 1,
        k_EAccountTypeMultiseat = 2,
        k_EAccountTypeGameServer = 3,
        k_EAccountTypeAnonGameServer = 4,
        k_EAccountTypePending = 5,
        k_EAccountTypeContentServer = 6,
        k_EAccountTypeClan = 7,
        k_EAccountTypeChat = 8,
        k_EAccountTypeConsoleUser = 9,
        k_EAccountTypeAnonUser = 10,
        k_EAccountTypeMax = 11,
    };
}
