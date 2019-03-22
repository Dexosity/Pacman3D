using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenGL_Game.Managers;
using OpenGL_Game.Scenes;
using OpenTK.Input;

namespace Game_Demo
{
    public class GameWin : Scene
    {
        public GameWin(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Winner!";
            sceneManager.Icon = Icon.ExtractAssociatedIcon("Textures/pacman.ico");
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
        }

        public override void Update(FrameEventArgs e)
        {
            // Checks if the player has presed enter (button to restart a new game)
            if (Keyboard.GetState().IsKeyDown(Key.Enter))
            { sceneManager.ChangeScene(1); }
            // Checks if the player wants to exit the game fully
            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            { sceneManager.Exit(); }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Width, sceneManager.Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 1);

            GUI.clearColour = Color.Firebrick;

            //Display the Title
            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 10f;
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f) + 128, (int)width, (int)(fontSize * 2f)), "Winner Winner!", (int)fontSize, StringAlignment.Center);
            GUI.DrawImage((int)(width / 2) - 64, (int)(height / 2), "Textures/pacman.png", false, new Vector2(2.0f));
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f) + 512, (int)width, (int)(fontSize * 2f)), "Press ENTER to play again!", (int)(fontSize / 5), StringAlignment.Center);
            GUI.Render();
        }

        public override void Close()
        {
        }
    }
}