using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class Constant
    {
        public const int SaltRound = 12;

        public const int AdminRole = 1;
        public const int UserRole = 2;
        // Manager of club
        public const int BusinessRole = 3; 
        // Manager of Web
        public const int BusinessManager = 4;

        public const string StrBusinessRole = "Business";
        public const string StrUserRole = "User";
        public const string StrAdminRole = "Admin";
        public const string StrBusinessManagerRole = "BusinessManager";

        public const int TournamentIncoming = 0;
        public const int TournamentInProgress = 1;
        public const int TournamentComplete = 2;
        public const byte CreateTourStepOne = 11;
        public const byte CreateTourStepTwo = 12;
        public const byte CreateTourStepThree = 13;
        public const byte CreateTourStepFour = 14;
        public const byte CreateTourStepFive = 15;
        public const byte CreateTourStepSix = 16;

        public const int AccessPublic = 1;
        public const int AccessPrivate = 2;

        public const int SysRandomDraw = 1;
        public const int UserRandomDraw = 2;
        public const int UserCustom = 3;

        public const int Game8Ball = 1;
        public const int Game9Ball = 2;
        public const int Game10Ball = 3;
        public const string String8Ball = "8 bi";
        public const string String9Ball = "9 bi";
        public const string String10Ball = "10 bi";

        public const int SingleEliminate = 1;
        public const int DoubleEliminate = 2;

        /// <summary>
        /// Status of account
        /// </summary>
        public const int AccountStatusReady = 0;
        public const int AccountStatusVerify = 1;
        public const int AccountStatusBanned = 2;

        /// <summary>
        /// Default country for player
        /// </summary>
        public const int NationVietNamId = 1;

        /// <summary>
        /// Status of match
        /// </summary>
        public const byte MatchIncoming = 0;
        public const byte MatchInprogress = 1;
        public const byte MatchEnd = 2;
        public const byte MatchTBD = 3;

        /// <summary>
        /// Status of a table
        /// </summary>
        public const bool TableInUse = true;
        public const bool TableNotUse = false;

        /// <summary>
        /// Status of player
        /// </summary>
        public const int NormalPlayer = 0;
        public const int SurrenderPlayer = 1;
        public const int GiveUpPlayer = 2;
        // player bị loại 
        public const int EliminatedPlayer = 3;
        // player bị reject
        public const int RejectedPlayer = 4;

        // không có trận tiếp theo W => vô địch, L => bị loại
        public const int NoNextMatch = 0;

        public const int ScoreForSurrender = 0;
        public const int ScoreForWinner = 1;

        public const int NumberPlayerOfMatch = 2;
        public const string BotPlayerName = "BOT";
    }
}
