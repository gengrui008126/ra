namespace RAGENativeUI.ImGui
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Runtime.CompilerServices;

    using Rage;
    using Rage.Native;
    using Graphics = Rage.Graphics;

    internal static partial class Gui
    {
        public delegate void GuiEventHandler();
        
        private static State state;
        private static Texture mouseTexture;

        public static event GuiEventHandler Do;

        static Gui()
        {
            Game.FrameRender += OnFrameRender;
        }

        private static void OnFrameRender(object sender, GraphicsEventArgs e)
        {
            if (Do != null)
            {
                state.ResetIds();
                
                state.HasMouseBeenCalled = false;
                state.Graphics = e.Graphics;

                Delegate[] delegates = Do.GetInvocationList();
                for (int i = 0; i < delegates.Length; i++)
                {
                    state.IsMouseEnabled = false;
                    state.ScreenContainer = new Container(null, new RectangleF(0f, 0f, Game.Resolution.Width, Game.Resolution.Height));
                    state.CurrentContainer = state.ScreenContainer;

                    ((GuiEventHandler)delegates[i]).Invoke();
                }

                if (state.HasMouseBeenCalled)
                {
                    if (mouseTexture == null)
                    {
                        mouseTexture = /** REDACTED **/;
                    }

                    state.Graphics.DrawTexture(mouseTexture, state.MousePosition.X, state.MousePosition.Y, 32.0f, 32.0f);
                }
            }
            state.Graphics = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCall()
        {
            if(state.Graphics == null)
                throw new InvalidOperationException("Gui methods cannot be called from outside the Gui.Do event.");
        }

        public static Vector2 Mouse(bool disableGameControls = true)
        {
            EnsureCall();

            state.IsMouseEnabled = true;
            if (!state.HasMouseBeenCalled)
            {
                state.LastMouseState = state.CurrentMouseState;
                state.CurrentMouseState = /** REDACTED **/;
                state.HasMouseBeenCalled = true;
            }
            state.MousePosition = /** REDACTED **/;

            if (disableGameControls)
            {
                NativeFunction.Natives.DisableAllControlActions(0);
            }

            return state.MousePosition;
        }
        
        public static void BeginWindow(ref RectangleF position, string title)
        {
            position = BeginWindow(position, title);
        }

        public static RectangleF BeginWindow(RectangleF position, string title)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));

            EnsureCall();
            uint id = state.Id(true, false);

            const float TitleBarHeight = 30f;


            Container container = state.CurrentContainer;
            Vector2 drawPos = container.ConvertToRootCoords(new Vector2(position.Location.X, position.Location.Y));

            state.PushContainer(new RectangleF(position.X, position.Y + TitleBarHeight, position.Width, position.Height - TitleBarHeight));

            RectangleF titleBarRect = new RectangleF(drawPos.X, drawPos.Y, position.Width, TitleBarHeight);
            RectangleF windRect = new RectangleF(drawPos.X, drawPos.Y + TitleBarHeight, position.Width, position.Height - TitleBarHeight);

            state.Graphics.DrawRectangle(titleBarRect, Color.FromArgb(215, 15, 15, 15));
            state.Graphics.DrawRectangle(windRect, Color.FromArgb(150, 45, 45, 45));

            DrawText(titleBarRect, title, 20f);

            DrawTextDebug(new Vector2(titleBarRect.X, titleBarRect.Y), $"Window {id.ToString("X8")}", 18.0f);

            if (state.IsMouseEnabled)
            {
                if (state.IsDragging(id))
                {
                    if (state.CurrentMouseState.IsLeftButtonUp)
                    {
                        state.Drop();
                    }
                    else
                    {
                        Vector2 offset = state.DragOffset();
                        float newX = position.Location.X + offset.X;
                        float newY = position.Location.Y + offset.Y;
                        newX = MathHelper.Clamp(newX, 0f, container.DrawArea.Width - position.Width);
                        newY = MathHelper.Clamp(newY, 0f, container.DrawArea.Height - position.Height);
                        position.Location = new PointF(newX, newY);
                    }
                }
                else if(!state.IsDraggingAny() && titleBarRect.Contains(state.MousePosition.X, state.MousePosition.Y) && /** REDACTED **/)
                {
                    state.Drag(id);
                }
            }

            return position;
        }

        public static void EndWindow()
        {
            state.PopContainer();
        }

        public static bool Button(RectangleF position, string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            EnsureCall();
            uint id = state.Id();

            RectangleF drawPos = state.CurrentContainer.ConvertToRootCoords(position);
            RectangleF clip = state.CurrentContainer.ConvertToRootCoords(state.CurrentContainer.ClipLocalRectangle(position));

            bool hovered = false;
            bool down = false;

            RectangleF buttonRect = RectangleF.Intersect(drawPos, clip);

            if (state.IsMouseEnabled)
            {
                hovered = buttonRect.Contains(state.MousePosition.X, state.MousePosition.Y);
                if(hovered)
                {
                    down = /** REDACTED **/;
                }
            }

            state.Graphics.DrawRectangle(buttonRect, hovered ? down ? Color.FromArgb(245, 45, 45, 45) : Color.FromArgb(240, 25, 25, 25) : Color.FromArgb(230, 10, 10, 10));
            
            DrawText(drawPos, clip, text);

            DrawTextDebug(new Vector2(drawPos.X, drawPos.Y), $"Button {id.ToString("X8")}", 18.0f);

            return down;
        }

        public static void Toggle(RectangleF position, string text, ref bool value)
        {
            value = Toggle(position, text, value);
        }

        public static bool Toggle(RectangleF position, string text, bool value)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            EnsureCall();
            uint id = state.Id();

            RectangleF drawPos = state.CurrentContainer.ConvertToRootCoords(position);
            RectangleF clip = state.CurrentContainer.ConvertToRootCoords(state.CurrentContainer.ClipLocalRectangle(position));

            bool hovered = false;
            bool down = false;


            RectangleF bgRect = RectangleF.Intersect(new RectangleF(drawPos.X, drawPos.Y, drawPos.Height, drawPos.Height), clip);

            if (state.IsMouseEnabled)
            {
                hovered = bgRect.Contains(state.MousePosition.X, state.MousePosition.Y);
                if (hovered)
                {
                    if (Game.WasKeyJustPressed(Keys.LButton))
                    {
                        down = true;
                        value = !value;
                    }
                }
            }
            
            state.Graphics.DrawRectangle(bgRect, Color.FromArgb(230, 10, 10, 10));
            if (value)
            {
                RectangleF rect = RectangleF.Intersect(new RectangleF(drawPos.X + 3f, drawPos.Y + 3f, drawPos.Height - 6f, drawPos.Height - 6f), clip);
                state.Graphics.DrawRectangle(rect, hovered ? down ? Color.FromArgb(240, 95, 95, 95) : Color.FromArgb(240, 70, 70, 70) : Color.FromArgb(240, 55, 55, 55));
            }
            
            DrawText(drawPos, clip, text, 15.0f, TextHorizontalAligment.Right, TextVerticalAligment.Center);

            DrawTextDebug(new Vector2(drawPos.X, drawPos.Y), $"Toggle {id.ToString("X8")}", 18.0f);

            return value;
        }

        public static void Label(RectangleF rectangle, string text, float fontSize = 15.0f, TextHorizontalAligment hAlign = TextHorizontalAligment.Left, TextVerticalAligment vAlign = TextVerticalAligment.Center)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            EnsureCall();
            uint id = state.Id();

            RectangleF drawPos = state.CurrentContainer.ConvertToRootCoords(rectangle);
            RectangleF clip = state.CurrentContainer.ConvertToRootCoords(state.CurrentContainer.ClipLocalRectangle(rectangle));

            DrawText(drawPos, clip, text, fontSize, hAlign, vAlign);

            DrawTextDebug(new Vector2(drawPos.X, drawPos.Y), $"Label {id.ToString("X8")}", 18.0f);
            DrawRectangleDebug(drawPos);
        }

        public static void HorizontalSlider(RectangleF rectangle, ref float value, float minValue, float maxValue)
        {
            value = HorizontalSlider(rectangle, value, minValue, maxValue);
        }

        public static float HorizontalSlider(RectangleF rectangle, float value, float minValue, float maxValue)
        {
            EnsureCall();
            uint id = state.Id();

            RectangleF drawPos = state.CurrentContainer.ConvertToRootCoords(rectangle);
            RectangleF clip = state.CurrentContainer.ConvertToRootCoords(state.CurrentContainer.ClipLocalRectangle(rectangle));
            float handleSize = drawPos.Height - 6;
            float handleRelativePos = (value - minValue) / (maxValue - minValue);
            RectangleF handleRect = new RectangleF(handleRelativePos * (drawPos.Width - handleSize - 6) + 3 + drawPos.Location.X, drawPos.Location.Y + 3, handleSize, handleSize);
            handleRect = RectangleF.Intersect(handleRect, clip);

            bool hovered = false;
            bool down = false;

            if (state.IsMouseEnabled)
            {
                if (state.IsDragging(id))
                {
                    if (state.CurrentMouseState.IsLeftButtonUp)
                    {
                        state.Drop();
                    }
                    else
                    {
                        down = true;
                        hovered = true;

                        float handlePos = handleRect.X;

                        float offset = state.DragOffset().X;

                        if (offset != 0)
                        {
                            value = MathHelper.Clamp(value + (offset * (maxValue - minValue) / drawPos.Width), minValue, maxValue);
                        }
                    }
                }
                else if (!state.IsDraggingAny() && handleRect.Contains(state.MousePosition.X, state.MousePosition.Y))
                {
                    hovered = true;
                    if (Game.WasKeyJustPressed(Keys.LButton))
                    {
                        down = true;
                        state.Drag(id);
                    }
                }
            }

            state.Graphics.DrawRectangle(RectangleF.Intersect(drawPos, clip), Color.FromArgb(230, 10, 10, 10));
            state.Graphics.DrawRectangle(handleRect, hovered ? down ? Color.FromArgb(240, 95, 95, 95) : Color.FromArgb(240, 70, 70, 70) : Color.FromArgb(240, 55, 55, 55));

            DrawTextDebug(new Vector2(drawPos.X, drawPos.Y), $"HSlider {id.ToString("X8")}", 18.0f);

            return value;
        }

        public static void VerticalSlider(RectangleF rectangle, ref float value, float minValue, float maxValue)
        {
            value = VerticalSlider(rectangle, value, minValue, maxValue);
        }

        public static float VerticalSlider(RectangleF rectangle, float value, float minValue, float maxValue)
        {
            EnsureCall();
            uint id = state.Id();

            RectangleF drawPos = state.CurrentContainer.ConvertToRootCoords(rectangle);
            RectangleF clip = state.CurrentContainer.ConvertToRootCoords(state.CurrentContainer.ClipLocalRectangle(rectangle));
            float handleSize = drawPos.Width - 6;
            float handleRelativePos = (value - minValue) / (maxValue - minValue);
            RectangleF handleRect = new RectangleF(drawPos.Location.X + 3, handleRelativePos * (drawPos.Height - handleSize - 6) + 3 + drawPos.Location.Y, handleSize, handleSize);
            handleRect = RectangleF.Intersect(handleRect, clip);

            bool hovered = false;
            bool down = false;

            if (state.IsMouseEnabled)
            {
                if (state.IsDragging(id))
                {
                    if (state.CurrentMouseState.IsLeftButtonUp)
                    {
                        state.Drop();
                    }
                    else
                    {
                        down = true;
                        hovered = true;

                        float handlePos = handleRect.Y;

                        float offset = state.DragOffset().Y;

                        if (offset != 0)
                        {
                            value = MathHelper.Clamp(value + (offset * (maxValue - minValue) / drawPos.Height), minValue, maxValue);
                        }
                    }
                }
                else if (!state.IsDraggingAny() && handleRect.Contains(state.MousePosition.X, state.MousePosition.Y))
                {
                    hovered = true;
                    if (Game.WasKeyJustPressed(Keys.LButton))
                    {
                        down = true;
                        state.Drag(id);
                    }
                }
            }

            state.Graphics.DrawRectangle(RectangleF.Intersect(drawPos, clip), Color.FromArgb(230, 10, 10, 10));
            state.Graphics.DrawRectangle(handleRect, hovered ? down ? Color.FromArgb(240, 95, 95, 95) : Color.FromArgb(240, 70, 70, 70) : Color.FromArgb(240, 55, 55, 55));

            DrawTextDebug(new Vector2(drawPos.X, drawPos.Y), $"VSlider {id.ToString("X8")}", 18.0f);

            return value;
        }

        public static void HorizontalSlider(RectangleF rectangle, ref int value, int minValue, int maxValue)
        {
            value = HorizontalSlider(rectangle, value, minValue, maxValue);
        }

        public static int HorizontalSlider(RectangleF rectangle, int value, int minValue, int maxValue)
        {
            return (int)HorizontalSlider(rectangle, (float)value, (float)minValue, (float)maxValue);
        }

        public static void VerticalSlider(RectangleF rectangle, ref int value, int minValue, int maxValue)
        {
            value = VerticalSlider(rectangle, value, minValue, maxValue);
        }

        public static int VerticalSlider(RectangleF rectangle, int value, int minValue, int maxValue)
        {
            return (int)VerticalSlider(rectangle, (float)value, (float)minValue, (float)maxValue);
        }

        public static void BeginScrollView(RectangleF position, ref Vector2 scrollPosition, SizeF viewSize, bool horizontalScrollbar = true, bool verticalScrollbar = true)
        {
            scrollPosition = BeginScrollView(position, scrollPosition, viewSize, horizontalScrollbar, verticalScrollbar);
        }

        public static Vector2 BeginScrollView(RectangleF position, Vector2 scrollPosition, SizeF viewSize, bool horizontalScrollbar = true, bool verticalScrollbar = true)
        {
            EnsureCall();
            uint id = state.Id(true, false);

            const float ScrollbarsSize = 17.0f;

            float x = horizontalScrollbar ? HorizontalSlider(new RectangleF(position.X, position.Bottom - ScrollbarsSize, position.Width - ScrollbarsSize, ScrollbarsSize), scrollPosition.X, 0f, viewSize.Width) : scrollPosition.X;
            float y = verticalScrollbar ? VerticalSlider(new RectangleF(position.Right - ScrollbarsSize, position.Y, ScrollbarsSize, position.Height - ScrollbarsSize), scrollPosition.Y, 0f, viewSize.Height) : scrollPosition.Y;

            RectangleF bgRect = state.CurrentContainer.ConvertToRootCoords(new RectangleF(position.X, position.Y, position.Width - (horizontalScrollbar ? ScrollbarsSize : 0.0f), position.Height - (verticalScrollbar ? ScrollbarsSize : 0.0f)));
            state.Graphics.DrawRectangle(bgRect, Color.FromArgb(180, 25, 25, 25));

            DrawTextDebug(new Vector2(bgRect.X, bgRect.Y), $"ScrollView {id.ToString("X8")}", 18.0f);

            state.PushContainer(new RectangleF(position.X, position.Y, position.Width - (horizontalScrollbar ? ScrollbarsSize : 0.0f), position.Height - (verticalScrollbar ? ScrollbarsSize : 0.0f)), new PointF(-scrollPosition.X, -scrollPosition.Y));

            return new Vector2(x, y);
        }

        public static void EndScrollView()
        {
            state.PopContainer();
        }


        private static void DrawText(RectangleF rectangle, string text, float fontSize = 15.0f, TextHorizontalAligment hAlign = TextHorizontalAligment.Center, TextVerticalAligment vAlign = TextVerticalAligment.Center)
        {
            DrawText(rectangle, rectangle, text, fontSize, hAlign, vAlign);
        }

        private static void DrawText(RectangleF rectangle, RectangleF clipRectangle, string text, float fontSize = 15.0f, TextHorizontalAligment hAlign = TextHorizontalAligment.Center, TextVerticalAligment vAlign = TextVerticalAligment.Center)
        {
            RectangleF textSize = Graphics.MeasureText(text, "Consolas", fontSize);
            float x = 0.0f, y = 0.0f;

            switch (hAlign)
            {
                case TextHorizontalAligment.Left:
                    x = rectangle.X;
                    break;
                case TextHorizontalAligment.Center:
                    x = rectangle.X + rectangle.Width * 0.5f - textSize.Width * 0.5f;
                    break;
                case TextHorizontalAligment.Right:
                    x = rectangle.Right - textSize.Width - 2.0f;
                    break;
            }

            switch (vAlign)
            {
                case TextVerticalAligment.Top:
                    y = rectangle.Y;
                    break;
                case TextVerticalAligment.Center:
                    y = rectangle.Y + rectangle.Height * 0.5f - textSize.Height * 0.8f;
                    break;
                case TextVerticalAligment.Down:
                    y = rectangle.Y + rectangle.Height - textSize.Height * 1.6f;
                    break;
            }

            state.Graphics.DrawText(text, "Consolas", fontSize, new Vector2(x, y), Color.White, clipRectangle);
        }

        [Conditional("DEBUG")]
        private static void DrawTextDebug(Vector2 position, string text, float fontSize = 15.0f)
        {
            if (Game.IsShiftDown)
            {
                state.Graphics.DrawText(text, "Consolas", fontSize, position, Color.Red);
            }
        }

        [Conditional("DEBUG")]
        private static void DrawRectangleDebug(RectangleF position)
        {
            if (Game.IsShiftDown)
            {
                state.Graphics.DrawRectangle(position, Color.FromArgb(50, 255, 0, 0));
            }
        }
    }
}
