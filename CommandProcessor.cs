using System;
using System.Net.Sockets;

namespace Server
{
    public class CommandProcessor
    {   
        public static void CommandHandler(string command, string json, Socket socket = null)
        {
            switch(command)
            {
                case string a when a.Contains("GetServers"): 
                    GetServersService getServers = new GetServersService();
                    getServers.GetServers(socket, command);
                    break;
                case string b when b.Contains("Login"): 
                    LoginService loginService = new LoginService();
                    loginService.LoginAsync(socket, command, json);
                    break;
                case string b when b.Contains("GetInventory"): 
                    GetInventoryService getInventory = new GetInventoryService();
                    getInventory.InventoryAsync(socket, command, json, false);
                    break;
                case string b when b.Contains("GetEquipmentInventory"): 
                    GetInventoryService getEquipmentInventory = new GetInventoryService();
                    getEquipmentInventory.InventoryAsync(socket, command, json, true);
                    break;
                case string b when b.Contains("WearCommand"): 
                    WearCommandService wearCommandService = new WearCommandService();
                    wearCommandService.WearCommand(socket, command, json);
                    break;
            }
        }
       
    }
}