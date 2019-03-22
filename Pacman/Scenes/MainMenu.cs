using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenGL_Game.Managers;
using OpenGL_Game.Scenes;
using OpenTK.Input;
using OpenGL_Game.Objects;
using OpenTK.Audio;

namespace Game_Demo
{
    public class MainMenu : Scene
    {
        // Play Button values
        float scaler = 1.0f;
        int sign = 1;
        bool ShowPlay = true;
        // Transition values
        bool Transition = false;
        float TransitionX = 0.0f;
        // Audio values
        Audio play;
        AudioContext audioContext = new AudioContext();

        public MainMenu(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Main Menu";
            sceneManager.Icon = Icon.ExtractAssociatedIcon("Textures/pacman.ico");
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            // Scene is now active
            sceneManager.Mouse.ButtonDown += Mouse_ButtonDown;
            // Frees mouse for user
            sceneManager.ShowCursor = true;
            sceneManager.MouseControlledCamera = false;
        }

        public override void Update(FrameEventArgs e)
        {
            // Checks if the player wants to exit the game fully
            if (GamePad.GetState(1).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Key.Escape))
            { sceneManager.Exit(); }

            float dt = (float)e.Time;
            // Updates the font size scaler by deltatime and a speed value (50.0f)
            scaler += dt * sign * 50.0f;
            // This created an upper and lower limit for the font size scaler
            // sign int is used to manage if it increases or decreases
            if(scaler > 30.0f)
            { sign *= -1; }
            if(scaler < -10.0f)
            { sign *= -1; }
            // Incremented the transition's X postion using deltatime if transition is active
            if (Transition)
            { TransitionX += dt * 2000.0f; }
            // Checks if the transition is complete and changed to the next scene if true
            if(TransitionX > sceneManager.Width + 50.0f)
            {
                sceneManager.NextScene();
                play.Close();
            }
            // The check that see's if the transition is over the play button
            if(TransitionX > sceneManager.Width / 4)
            { ShowPlay = false; }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 100);

            GUI.clearColour = Color.Firebrick;

            
            //Display the Title
            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 10f;
            int middlex = (int)(width / 2), middley = (int)(height / 2);
            // Draws scene title
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f), (int)width, (int)(fontSize * 2f)), "Pacman 3D", (int)fontSize, StringAlignment.Center);
            // Draws two images on either side of the main text (above text)
            GUI.DrawImage(middlex - 512, (int)(fontSize / 2f) + 26, "Textures/pacman.png", false, new Vector2(2.0f));
            GUI.DrawImage(middlex + 400, (int)(fontSize / 2f) + 26, "Textures/pacman.png", true, new Vector2(2.0f));
            // Checks if transition has 'eaten' play button - if not then manipulate font size by scaler
            if (ShowPlay) { GUI.Label(new Rectangle(0, middley, (int)width, (int)(fontSize * 2f)), "PLAY", (int)fontSize + (int)scaler, StringAlignment.Center); }
            // Checks if it's time to transition and draws the transition and manipulates its x by the transitionX value
            if (Transition) { GUI.DrawImage((int)TransitionX, (int)(fontSize / 2f) + 180, "Textures/pacman.png", false, new Vector2(10.0f)); }
            GUI.Render();
        }

        private void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
                // Checks if the player has clicked the mouse to start playing the game
                case MouseButton.Left:
                case MouseButton.Right:
                    // Plays sound effect while tranisition is active
                    play = new Audio(ResourceManager.LoadAudio("Audio/sfx_menu_select4.wav"));
                    play.PlayAudio();
                    Transition = true;
                    break;
            }
        }

        public override void Close()
        {
            audioContext.Dispose();
            sceneManager.Mouse.ButtonDown -= Mouse_ButtonDown;
        }
    }
}