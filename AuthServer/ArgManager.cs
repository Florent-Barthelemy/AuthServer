using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer
{
    public static class ArgManager
    {
        static List<CommandLineArg> registeredArgs = new List<CommandLineArg>();

        /// <summary>
        /// Parse a string[] containing args and calls the callbacks
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool ParseArgs(string[] inputArgs)
        {
            //Cycling through all args
            for (int i = 0; i < inputArgs.Length; i++)
            {
                //Compare each arg to all registrations
                foreach (CommandLineArg arg in registeredArgs)
                {
                    //If the argument matches the registration, call the cbk
                    //With a new Arg context
                    if (arg.CompareToString(inputArgs[i]))
                    {
                        string[] newArgContext = new string[arg.argNumber];

                        //Passing only the option arguments to the callback
                        try { Utils.CopyArray(inputArgs, i + 1, ref newArgContext, 0, arg.argNumber); }
                        catch (Exception ex) { throw new SystemException("Failed to parse cli args", ex); }

                        arg.callback(newArgContext);

                        i += arg.argNumber;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Registers a command line argument
        /// </summary>
        /// <param name="argName">Command name (with -)</param>
        /// <param name="argNumber">Number off next args to pass in the callback after the command call</param>
        /// <param name="callback"></param>
        public static void RegisterArg(string argName, int argArgsCount, ArgCallback callback, string helpText, string aliasName = "", bool isMendatory = false)
        {
            registeredArgs.Add(new CommandLineArg(argName, aliasName, helpText, argArgsCount, callback, isMendatory));
        }

        /// <summary>
        /// Returns a string that contains a formatted help menu
        /// </summary>
        /// <returns>a string</returns>
        public static string GetHelpMenu()
        {
            string helpMenu = "";
            for (int i = 0; i < registeredArgs.Count; i++)
            {
                CommandLineArg cliArg = registeredArgs.ElementAt(i);
                helpMenu += "     " + cliArg.argName + " , " + cliArg.aliasName + "  " + cliArg.helpText;
                helpMenu += "\n";
            }

            return helpMenu;
        }

        /// <summary>
        /// A function to call when an argument is found, the manager passes a string[] that 
        /// corresponds to a nex context
        /// </summary>
        /// <param name="argArgs">Following parameters of the command</param>
        public delegate void ArgCallback(string[] argArgs);
    }

    /// <summary>
    /// Models a command line argument
    /// </summary>
    public class CommandLineArg
    {
        public string argName;
        public string helpText;
        public string aliasName;
        bool hasAlias = false;

        public int argNumber;
        public ArgManager.ArgCallback callback;

        public bool isMendatory = false;

        /// <summary>
        /// Creates a new CommandLineArg model
        /// </summary>
        /// <param name="ArgName"></param>
        /// <param name="AliasName">set AliasName to ( "" ) to set no aliases</param>
        /// <param name="ArgsArgNumber">Number of args after arg command</param>
        public CommandLineArg(string ArgName, string AliasName, string Helptext, int ArgsArgNumber, ArgManager.ArgCallback Callback, bool IsMendatory = false)
        {
            argName = ArgName;
            aliasName = AliasName;
            hasAlias = AliasName != "";
            argNumber = ArgsArgNumber;
            callback = Callback;
            helpText = Helptext;
            isMendatory = IsMendatory;
        }

        /// <summary>
        /// Compares the argument and its alias to a 
        /// </summary>
        /// <param name="input">the string to compare</param>
        /// <returns>Either or not the string matches its names or its alias</returns>
        public bool CompareToString(string input)
        {
            return (input == argName || (hasAlias && (input == aliasName)));
        }
    }
}
