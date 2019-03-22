using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenGL_Game.Managers;
using OpenGL_Game.Scenes;
using OpenTK.Input;

namespace Game_Demo
{
    public class GameOver : Scene
    {
        public GameOver(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Game Over!";
            sceneManager.Icon = Icon.ExtractAssociatedIcon("Textures/pacman.ico");
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
        }

        public override void Update(FrameEventArgs e)
        {
            // Checks to see if the player wants to exit the game fully
            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            { sceneManager.Exit(); }
            // Checks if the user has pressed enter (button to restart a new game)
            if (Keyboard.GetState().IsKeyDown(Key.Enter))
            { sceneManager.ChangeScene(1); }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Width, sceneManager.Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 1);

            GUI.clearColour = Color.DarkSlateGray;

            //Display the Title
            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 10f;
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f) + 128, (int)width, (int)(fontSize * 2f)), "GAME OVER!", (int)fontSize, StringAlignment.Center);
            GUI.DrawImage((int)(width / 2) - 64, (int)(height / 2), "Textures/pacmandead.png", false, new Vector2(2.0f));
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f) + 512, (int)width, (int)(fontSize * 2f)), "Press ENTER to play again!", (int)(fontSize / 5), StringAlignment.Center);

            GUI.Render();
        }

        public override void Close()
        {
        }
    }
}