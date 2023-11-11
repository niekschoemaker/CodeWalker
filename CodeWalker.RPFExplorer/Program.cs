﻿using CodeWalker.GameFiles;
using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.Utils;
using CodeWalker.Core.Utils;

namespace CodeWalker.RPFExplorer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                if (!NamedPipe.TrySendMessageToOtherProcess("explorer"))
                {
                    ConsoleWindow.Hide();
                    //Process.Start("CodeWalker.exe", "explorer");
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    var form = new ExploreForm();
                    var namedPipe = new NamedPipe(form);
                    namedPipe.Init();

                    Application.Run(form);

                    GTAFolder.UpdateSettings();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }


        }
    }
}
