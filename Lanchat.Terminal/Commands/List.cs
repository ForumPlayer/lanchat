﻿using System;
using System.Collections.Generic;
using ConsoleGUI.Controls;
using Lanchat.Core.Models;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands
{
    public static class List
    {
        public static void Execute()
        {
            Program.Network.Nodes.ForEach(x =>
            {
                var status = new TextBlock();

                switch (x.Status)
                {
                    case Status.Online:
                        status.Text = "Online";
                        status.Color = ConsoleColor.Green;
                        break;

                    case Status.AwayFromKeyboard:
                        status.Text = "Afk";
                        status.Color = ConsoleColor.Yellow;
                        break;

                    case Status.DoNotDisturb:
                        status.Text = "Dnd";
                        status.Color = ConsoleColor.Red;
                        break;
                }

                var line = new[]
                {
                    new TextBlock {Text = $"{x.Nickname} (", Color = ConsoleColor.White},
                    status,
                    new TextBlock {Text = ")", Color = ConsoleColor.White},
                };

                Ui.Log.AddCustomTextBlock(line);
            });

            Program.Network.DetectedNodes.ForEach(x =>
            {
                var line = new[]
                {
                    new TextBlock {Text = $"{x.Nickname} (", Color = ConsoleColor.White},
                    new TextBlock {Text = "Detected", Color = ConsoleColor.DarkCyan},
                    new TextBlock {Text = ")", Color = ConsoleColor.White},
                };

                Ui.Log.AddCustomTextBlock(line);
            });
        }
    }
}