using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using TerrariaApi.Server;

namespace GriefReport
{
    [ApiVersion(1, 17)]
    public class Reports : TerrariaPlugin
    {
        #region plugin info
        public override Version Version
        {
            get { return new Version("1.0"); }
        }
        public override string Name
        {
            get { return "Reports"; }
        }
        public override string Author
        {
            get { return "Bippity"; }
        }
        public override string Description
        {
            get { return "Report griefs & Messages"; }
        }
        public Reports(Main game)
            : base(game)
        {
            Order = 4;
        }
        #endregion

        #region Initialize/Dispose
        private List<Grief> Griefs = new List<Grief>();
        private List<Message> Other = new List<Message>();
        public override void Initialize()
        {
            PlayerHooks.PlayerPostLogin += PostLogin;

           // Commands.ChatCommands.Add(new Command("report.report", Report, "report"));
           // Commands.ChatCommands.Add(new Command("report.check", Check, "checkreport"));
            Commands.ChatCommands.Add(new Command(new List<string>() { "report.report", "report.check" }, Report, "report"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PlayerHooks.PlayerPostLogin -= PostLogin;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Report
        private void PostLogin(PlayerPostLoginEventArgs args)
        {
            if (args.Player == null)
                return;
            TSPlayer player = args.Player;

            if (player.Group.HasPermission("report.check"))
            {
                player.SendWarningMessage("There are " + Griefs.Count + " reported griefs and " + Other.Count + " other reports. Type /report -check");
            }
        }

        public void Report(CommandArgs args)
        {
            if (args.Player == null)
                return;
            TSPlayer player = args.Player;
            if (args.Parameters.Count == 0)
            {
                player.SendErrorMessage("Invalid syntax! Proper syntax: /report <message/help/-grief/-check>");
                return;
            }

            var subcmd = args.Parameters[0].ToLower();

            switch (subcmd)
            {
                case "-grief":
                    if (player.Group.HasPermission("report.report"))
                    {
                        foreach (Grief report in Griefs)
                        {
                            int x = report.x;
                            int y = report.y;
                            if (x > player.TileX - 50 && x < player.TileX + 50 && y > player.TileY - 50 && y < player.TileY + 50)
                            {
                                player.SendInfoMessage("This location has already been reported.");
                                return;
                            }
                        }

                        Griefs.Add(new Grief(player.TileX, player.TileY, player.UserAccountName, DateTime.UtcNow));
                        player.SendInfoMessage("Your grief report has been sent to the staff. Thanks!");
                        Console.WriteLine(string.Format("{0} has reported a grief at: {1}, {2}", player.Name, player.TileX, player.TileY));

                        foreach (TSPlayer ts in TShock.Players)
                        {
                            if (ts != null)
                            {
                                if (ts.Group.HasPermission("report.check"))
                                {
                                    ts.SendWarningMessage(string.Format("{0} has reported a grief at: {1}, {2}. Type /report -check", player.Name, player.TileX, player.TileY));
                                }
                            }
                        }
                    }
                    break;

                case "-check":
                    if (player.Group.HasPermission("report.check"))
                    {
                        if (Griefs.Count == 0 && Other.Count == 0)
                        {
                            player.SendInfoMessage("There are currently no reports!");
                        }
                        else if (Griefs.Count > 0)
                        {
                            Grief temp = Griefs[0];
                            player.Teleport(temp.x * 16, temp.y * 16);
                            player.SendInfoMessage(string.Format("Reported by: {0} at {1}", temp.Name, temp.Date));
                            Griefs.Remove(temp);
                            player.SendWarningMessage("There are " + Griefs.Count + " reported griefs and " + Other.Count + " other reports left.");
                        }
                        else
                        {
                            Message temp = Other[0];
                            player.Teleport(temp.x * 16, temp.y * 16);
                            player.SendInfoMessage(string.Format("Reported by: {0} at {1} with a message...", temp.Name, temp.Date));
                            player.SendMessage(string.Format("Message: \"{0}\"", temp.Report), Color.Green);
                            Other.Remove(temp);
                            player.SendWarningMessage("There are " + Griefs.Count + " reported griefs and " + Other.Count + " other reports left.");
                        }
                    }
                    break;

                case "help":
                    player.SendErrorMessage("Proper syntax: /report <message/help/-grief/-check>");
                    player.SendInfoMessage("If you see a grief, type: /report -grief");
                    player.SendInfoMessage("If you want to notify a staff about something, type: /report <your message>");
                    break;

                default:
                    if (player.Group.HasPermission("report.report"))
                    {
                        string str = string.Join(" ", args.Parameters);
                        if (str.Length <= 5)
                        {
                            player.SendErrorMessage("Your report message was too short!");
                            return;
                        }
                        Other.Add(new Message(player.TileX, player.TileY, player.UserAccountName, DateTime.UtcNow, str));
                        player.SendSuccessMessage("Your report has been sent to the staff. Thanks!");

                        foreach (TSPlayer ts in TShock.Players)
                        {
                            if (ts != null)
                            {
                                if (ts.Group.HasPermission("report.check"))
                                {
                                    ts.SendWarningMessage(string.Format("{0} has sent a report with a message just now. Type /report -check.", player.Name)); 
                                }
                            }
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}
