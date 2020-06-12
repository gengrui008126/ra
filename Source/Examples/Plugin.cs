﻿namespace RNUIExamples
{
    using System.Windows.Forms;
    using Rage;
    using RAGENativeUI;
    using RAGENativeUI.Elements;

    internal static class Plugin
    {
        public const string MenuTitle = "RAGENativeUI";

        public static MenuPool Pool { get; } = new MenuPool();
        private static UIMenu MainMenu { get; set; }

        private static void Main()
        {
            MainMenu = new UIMenu(MenuTitle, "SHOWCASE");
            Pool.Add(MainMenu);

            {
                UIMenuItem item = new UIMenuItem("Menus", $"Showcases the ~b~{nameof(UIMenu)}~s~ class");

                MainMenu.AddItem(item);
                MainMenu.BindMenuToItem(new MenuItems(), item);
            }

            {
                UIMenuItem item = new UIMenuItem("Timer Bars", "Showcases the available timer bars");

                MainMenu.AddItem(item);
                MainMenu.BindMenuToItem(new TimerBars(), item);
            }

            Game.RawFrameRender += (s, e) => Pool.DrawBanners(e.Graphics);

            while (true)
            {
                GameFiber.Yield();

                if (Game.IsKeyDown(Keys.F5))
                {
                    MainMenu.Visible = !MainMenu.Visible;
                }

                Pool.ProcessMenus();
            }
        }
    }
}