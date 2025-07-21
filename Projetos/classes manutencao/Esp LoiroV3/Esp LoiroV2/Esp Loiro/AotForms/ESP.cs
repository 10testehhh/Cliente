using AotForms;
using Client;
using ImGuiNET;
using System.Numerics;
using System.Runtime.InteropServices;
using System;
using System.Drawing;
using static AotForms.WinAPI;
using System.Collections.Generic;

namespace AotForms
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        private IntPtr hWnd;

        protected override unsafe void Render()
        {
            if (!Config.EspEnabled) return;

            if (!Core.HaveMatrix) return;
            Core.CurrentTime = (float)ImGui.GetTime();

            AimbotEsp.Render();
            CreateHandle();

            var drawList = ImGui.GetForegroundDrawList();

            ProcessAndDrawEntities(drawList);
        }

        #region Lógica Principal de Desenho

        private void ProcessAndDrawEntities(ImDrawListPtr drawList)
        {
            uint whiteColor = ColorToUint32(Color.White);
            uint blackColor = ColorToUint32(Color.Black);
            int enemyCount = 0;

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead || !entity.IsKnown)
                {
                    continue;
                }
                var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);

                var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;

                enemyCount++;

                float CornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
                float CornerWidth = (float)(CornerHeight * 0.65);

                if (Config.ESPLine)
                {
                    Vector2 lineStart = Config.EspUp ? new Vector2(Core.Width / 2f, 25f) :
                                          (Config.EspBottom ? new Vector2(Core.Width / 2f, Core.Height - 25f) : new Vector2(Core.Width / 2f, Core.Height / 2f));
                    Vector2 lineEnd = new Vector2(headScreenPos.X, headScreenPos.Y);
                    DrawGlowLine(lineStart, lineEnd, whiteColor, 1f, Config.GlowRadius, Config.FeatherAmount, Config.GlowOpacity);
                }

                if (Config.ESPBox)
                {
                    DrawCorneredBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, whiteColor, 1.5f);
                }

                if (Config.ESPName)
                {
                    var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "Bot" : entity.Name;
                    var namePosition = new Vector2(headScreenPos.X - (ImGui.CalcTextSize(nameText).X / 2), headScreenPos.Y - 20);
                    DrawSmallTextWithOutline(namePosition, nameText, whiteColor, blackColor);
                }

                if (Config.ESPDistance)
                {
                    string distanceText = $"{MathF.Round(dist)} M";
                    Vector2 distancePosition = new Vector2(bottomScreenPos.X - (ImGui.CalcTextSize(distanceText).X / 2), bottomScreenPos.Y + 5f);
                    DrawSmallTextWithOutline(distancePosition, distanceText, whiteColor, blackColor);
                }

                if (Config.ESPHealth)
                {
                    float healthBarX = headScreenPos.X - (CornerWidth / 2) - 10;
                    float healthBarY = headScreenPos.Y;
                    float healthBarHeight = CornerHeight;
                    DrawModernHealthBar(drawList, entity, healthBarX, healthBarY, healthBarHeight);
                }

                if (Config.ESPSkeleton)
                {
                    DrawSkeleton(entity, whiteColor);
                }
            }

            // Desenha o contador de inimigos
            string totalPlayersText = $"Enemy Detected: {enemyCount}";
            var totalPlayersTextSize = ImGui.CalcTextSize(totalPlayersText);
            var totalPlayersTextPosX = (Core.Width - totalPlayersTextSize.X) / 2;
            var totalPlayersTextPosY = 80;
            float padding = 10.0f;
            Vector2 bgRectMin = new Vector2(totalPlayersTextPosX - padding, totalPlayersTextPosY - padding);
            Vector2 bgRectMax = new Vector2(totalPlayersTextPosX + totalPlayersTextSize.X + padding, totalPlayersTextPosY + totalPlayersTextSize.Y + padding);
            uint bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 0.5f));
            drawList.AddRectFilled(bgRectMin, bgRectMax, bgColor, 5.0f);
            uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 0.8f));
            drawList.AddRect(bgRectMin, bgRectMax, borderColor, 5.0f);
            drawList.AddText(new Vector2(totalPlayersTextPosX, totalPlayersTextPosY), whiteColor, totalPlayersText);
        }
        #endregion

        #region Funções da Barra de Vida Moderna
        private void DrawModernHealthBar(ImDrawListPtr drawList, Entity entity, float x, float y, float height)
        {
            const float barWidth = 5f;
            const float maxHealth = 200f;

            float tempoDesdeDano = Core.CurrentTime - entity.TempoUltimoDano;
            float progressoAnimacao = Math.Clamp(tempoDesdeDano / 0.6f, 0f, 1f);

            float vidaAnimada = Lerp(entity.VidaAnterior, entity.Health, progressoAnimacao);

            if (progressoAnimacao >= 1.0f)
            {
                entity.VidaAnterior = entity.Health;
            }

            float healthPercentage = Math.Clamp(vidaAnimada / maxHealth, 0f, 1f);
            float barHeight = height * healthPercentage;

            drawList.AddRectFilled(new Vector2(x, y), new Vector2(x + barWidth, y + height), ColorToUint32(Color.FromArgb(100, 0, 0, 0)), 3f);

            if (barHeight > 0)
            {
                uint topColor = ColorToUint32(LerpColor(Color.Red, Color.LimeGreen, healthPercentage));
                uint bottomColor = ColorToUint32(LerpColor(Color.DarkRed, Color.Green, healthPercentage));
                drawList.AddRectFilledMultiColor(new Vector2(x, y + height - barHeight), new Vector2(x + barWidth, y + height), topColor, topColor, bottomColor, bottomColor);
            }

            string healthText = $"{(int)entity.Health}";
            Vector2 textSize = ImGui.CalcTextSize(healthText);
            drawList.AddText(new Vector2(x + (barWidth / 2) - (textSize.X / 2), y + height + 3), ColorToUint32(Color.White), healthText);
        }

        private float Lerp(float a, float b, float t) => a + (b - a) * t;

        private Color LerpColor(Color a, Color b, float t) => Color.FromArgb((int)Lerp(a.A, b.A, t), (int)Lerp(a.R, b.R, t), (int)Lerp(a.G, b.G, t), (int)Lerp(a.B, b.B, t));
        #endregion

        #region Outras Funções de Desenho
        void DrawSmallTextWithOutline(Vector2 pos, string text, uint textColor, uint outlineColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            vList.AddText(pos + new Vector2(1, 1), outlineColor, text);
            vList.AddText(pos + new Vector2(-1, -1), outlineColor, text);
            vList.AddText(pos + new Vector2(1, -1), outlineColor, text);
            vList.AddText(pos + new Vector2(-1, 1), outlineColor, text);
            vList.AddText(pos, textColor, text);
        }

        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, Math.Clamp(alpha, 0, 1)));
                drawList.AddLine(start, end, glowColor, thickness + (glowRadius - i) * 0.5f);
            }
            drawList.AddLine(start, end, color, thickness);
        }

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetForegroundDrawList();
            float lineW = W / 4;
            float lineH = H / 4;
            vList.AddLine(new Vector2(X, Y), new Vector2(X + lineW, Y), color, thickness);
            vList.AddLine(new Vector2(X, Y), new Vector2(X, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X + W, Y), new Vector2(X + W - lineW, Y), color, thickness);
            vList.AddLine(new Vector2(X + W, Y), new Vector2(X + W, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X, Y + H), new Vector2(X + lineW, Y + H), color, thickness);
            vList.AddLine(new Vector2(X, Y + H), new Vector2(X, Y + H - lineH), color, thickness);
            vList.AddLine(new Vector2(X + W, Y + H), new Vector2(X + W - lineW, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W, Y + H), new Vector2(X + W, Y + H - lineH), color, thickness);
        }

        private void DrawSkeleton(Entity entity, uint color)
        {
            var drawList = ImGui.GetForegroundDrawList();
            var head = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            var spine = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine, Core.Width, Core.Height);
            var hip = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip, Core.Width, Core.Height);
            var lShoulder = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftSholder, Core.Width, Core.Height);
            var rShoulder = W2S.WorldToScreen(Core.CameraMatrix, entity.RightSholder, Core.Width, Core.Height);
            var lElbow = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);
            var rElbow = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height);
            var lWrist = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);
            var rWrist = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var lCalf = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftCalf, Core.Width, Core.Height);
            var rCalf = W2S.WorldToScreen(Core.CameraMatrix, entity.RightCalf, Core.Width, Core.Height);
            var lFoot = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot, Core.Width, Core.Height);
            var rFoot = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);

            DrawLine(drawList, head, spine, color);
            DrawLine(drawList, spine, hip, color);
            DrawLine(drawList, spine, lShoulder, color);
            DrawLine(drawList, spine, rShoulder, color);
            DrawLine(drawList, lShoulder, lElbow, color);
            DrawLine(drawList, rShoulder, rElbow, color);
            DrawLine(drawList, lElbow, lWrist, color);
            DrawLine(drawList, rElbow, rWrist, color);
            DrawLine(drawList, hip, lCalf, color);
            DrawLine(drawList, hip, rCalf, color);
            DrawLine(drawList, lCalf, lFoot, color);
            DrawLine(drawList, rCalf, rFoot, color);
        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 start, Vector2 end, uint color)
        {
            if (start.X > 0 && start.Y > 0 && end.X > 0 && end.Y > 0)
            {
                drawList.AddLine(start, end, color, 1.5f);
            }
        }
        #endregion

        #region Funções Utilitárias
        static uint ColorToUint32(Color color)
        {
            return ImGui.ColorConvertFloat4ToU32(new Vector4(
                (float)(color.R / 255.0),
                (float)(color.G / 255.0),
                (float)(color.B / 255.0),
                (float)(color.A / 255.0)));
        }

        void CreateHandle()
        {
            hWnd = FindWindow(null, "Overlay");
            if (hWnd == IntPtr.Zero) return;

            IntPtr gameHWnd = Core.Handle;
            if (gameHWnd == IntPtr.Zero) return;

            RECT rect;
            GetWindowRect(gameHWnd, out rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width > 50 && height > 50)
            {
                Size = new Size(width, height);
                Position = new Point(rect.Left, rect.Top);
                Core.Width = width;
                Core.Height = height;

                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }
        }
        #endregion
    }
}
