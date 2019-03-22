using OpenGL_Game.Managers;
using OpenGL_Game.Objects;
using System;
using System.Collections.Generic;

namespace Pacman
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            // This creates a list for the game engine to have access to the custom scenes
            List<string> SceneList = new List<string>();
            // Alternatly you could ignore the config file and manualy type these in to pass to SceneManager
            WindowConfig config = new WindowConfig();
            // Loops through all scenes specified in the config
            for (int i = 0; i < config.Scenes.Length; i++)
            {
                Type type = Type.GetType("Game_Demo." + config.Scenes[i]);
                SceneList.Add(type.AssemblyQualifiedName);
            }
            // Initlises your game with all your specified scenes
            using (var game = new SceneManager(SceneList.ToArray()))
                game.Run();
        }
    }
}
