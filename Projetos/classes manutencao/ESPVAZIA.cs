using AotForms;
using ImGuiNET;
using System.Numerics;
using System.Runtime.InteropServices;
using static AotForms.WinAPI;

namespace AotForms
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        IntPtr hWnd;
        IntPtr HDPlayer;
        protected override unsafe void Render()
        {
            if (!Core.HaveMatrix) return;

            CreateHandle();
            string text = "";
            var textPosY = 80;
            var textSize = ImGui.CalcTextSize(text);
            uint textColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // White color
            var windowWidth = Core.Width;
            var windowHeight = Core.Height;

            var drawList = ImGui.GetForegroundDrawList();

            // Bloco do AimBotFov foi removido.

            var tmp = Core.Entities;

            // Handle window styles
            string windowName = "Overlay";
            hWnd = FindWindow(null, windowName);
            HDPlayer = FindWindow("BlueStacksApp", null);

            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }
            else
            {
                Console.WriteLine("The window was not found.");
            }

            Vector2 sharedCirclePosition = new Vector2(0, 0);
            sharedCirclePosition = new Vector2(windowWidth / 2f, 30);

            if (Config.minimap)
            {
                DrawMinimap();
            }
            int enemyCount = 0; // Initialize enemy count

            foreach (var entity in tmp.Values)
            {
                if (entity.IsDead || !entity.IsKnown)
                {
                    continue;
                }
                var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);

                // Verificação de distância (espran) foi removida.

                var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                enemyCount++;
                var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;
                if (bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;

                float CornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
                float CornerWidth = (float)(CornerHeight * 0.65);

                if (Config.ESPLine)
                {
                    Vector2 lineStart = Config.EspUp ? new Vector2(Core.Width / 2f, 25f) :
                                          (Config.EspBottom ? new Vector2(Core.Width / 2f, Core.Height - 25f) : new Vector2(Core.Width / 2f, Core.Height / 2f));
                    Vector2 lineEnd = new Vector2(headScreenPos.X, headScreenPos.Y);

                    float increasedGlowRadius = Config.GlowRadius * 2f;

                    if (entity.IsKnocked)
                    {
                        DrawGlowLine(lineStart, lineEnd, GetESPColor(ColorToUint32(Color.Red)), 1f, increasedGlowRadius, Config.FeatherAmount, Config.GlowOpacity);
                    }
                    else
                    {
                        DrawGlowLine(lineStart, lineEnd, GetESPColor(ColorToUint32(Config.ESPLineColor)), 1f, increasedGlowRadius, Config.FeatherAmount, Config.GlowOpacity);
                    }

                    if (Config.EspUp) DrawFilledCircle(25f, 3.0f);
                    if (Config.EspBottom) DrawFilledCircle(Core.Height - 25f, 3.0f);

                    float rotationSpeed = 0.3f;
                    float angle = (float)(Environment.TickCount * rotationSpeed % 360);
                    float angleRad = MathF.PI * angle / 180f;
                    float crosshairLength = 12f;
                    float time = Environment.TickCount * 0.002f;
                    Color rainbowColor = ColorFromHSV((time * 100) % 360, 1.0f, 1.0f);
                    uint rainbowColorUint = ColorToUint32(rainbowColor);
                    Vector2 crosshairTop = new Vector2(Core.Width / 2f + crosshairLength * MathF.Cos(angleRad), Core.Height / 2f - crosshairLength * MathF.Sin(angleRad));
                    Vector2 crosshairBottom = new Vector2(Core.Width / 2f - crosshairLength * MathF.Cos(angleRad), Core.Height / 2f + crosshairLength * MathF.Sin(angleRad));
                    Vector2 crosshairLeft = new Vector2(Core.Width / 2f - crosshairLength * MathF.Sin(angleRad), Core.Height / 2f - crosshairLength * MathF.Cos(angleRad));
                    Vector2 crosshairRight = new Vector2(Core.Width / 2f + crosshairLength * MathF.Sin(angleRad), Core.Height / 2f + crosshairLength * MathF.Cos(angleRad));
                    drawList.AddLine(crosshairTop, crosshairBottom, rainbowColorUint, 2.5f);
                    drawList.AddLine(crosshairLeft, crosshairRight, rainbowColorUint, 2.5f);
                    string displayText = "Loiro240hz";
                    Vector2 textPosition = new Vector2(Core.Width / 2f - 100f, Core.Height - 50f);
                    uint outlineColor = ColorToUint32(Color.Black);
                    DrawSmallTextWithOutline(textPosition, displayText, rainbowColorUint, outlineColor);
                }

                Color ColorFromHSV(float hue, float saturation, float value)
                {
                    int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
                    float f = hue / 60 - MathF.Floor(hue / 60);
                    float v = value * 255;
                    float p = v * (1 - saturation);
                    float q = v * (1 - f * saturation);
                    float t = v * (1 - (1 - f) * saturation);
                    return hi switch
                    {
                        0 => Color.FromArgb(255, (int)v, (int)t, (int)p),
                        1 => Color.FromArgb(255, (int)q, (int)v, (int)p),
                        2 => Color.FromArgb(255, (int)p, (int)v, (int)t),
                        3 => Color.FromArgb(255, (int)p, (int)q, (int)v),
                        4 => Color.FromArgb(255, (int)t, (int)p, (int)v),
                        _ => Color.FromArgb(255, (int)v, (int)p, (int)q),
                    };
                }

                if (Config.ESPFillBox)
                {
                    Color topColor = Color.FromArgb((int)(0.1f * 255), GetESPColor(Config.ESPFillBoxColor));
                    Color bottomColor = Color.FromArgb((int)(0.75f * 255), GetESPColor(Config.ESPFillBoxColor));
                    DrawGradientBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, topColor, bottomColor);
                }

                if (Config.ESPBox2)
                {
                    uint boxColor = GetESPColor(ColorToUint32(Config.ESPBoxColor));
                    float feather = 1.7f;
                    float glowOpacityMultiplier = 0.02f;
                    Draw3dBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f, Config.BoxGlow ? 1f : 0f, feather, glowOpacityMultiplier);
                }

                if (Config.ESPBox)
                {
                    uint boxColor = GetESPColor(ColorToUint32(Config.ESPBoxColor));
                    DrawCorneredBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);
                }

                var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "Bot" : entity.Name;
                var namePosition = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 28);

                if (Config.ESPName)
                {
                    DrawSmallTextWithOutline(namePosition, nameText, GetESPColor(ColorToUint32(Color.White)), ColorToUint32(Color.Black));
                }
                if (Config.ESPDistance)
                {
                    string distanceText = $"{MathF.Round(dist)} M";
                    float estimatedTextWidth = distanceText.Length * 8f;
                    Vector2 distancePosition = new Vector2(bottomScreenPos.X + 1 - (estimatedTextWidth / 2), bottomScreenPos.Y + 5f);
                    DrawSmallTextWithOutline(distancePosition, distanceText, GetESPColor(ColorToUint32(Config.ESPSkeletonColor)), ColorToUint32(Color.Black));
                }

                if (Config.ESPHealth)
                {
                    if (!entity.IsKnocked)
                    {
                        DrawHealthBar(entity.Health, 200, headScreenPos.X - (CornerWidth / 2) - 6, headScreenPos.Y, CornerHeight, 1.9f);
                    }
                    else
                    {
                        DrawHealthBarK(entity.Health, 200, headScreenPos.X - (CornerWidth / 2) - 6, headScreenPos.Y, CornerHeight, 1.9f);
                    }
                }

                if (Config.ESPSkeleton)
                {
                    DrawSkeleton(entity);
                }

                string totalPlayersText = $"Enemy Detected: {enemyCount}";
                var totalPlayersTextSize = ImGui.CalcTextSize(totalPlayersText);
                var totalPlayersTextPosX = (windowWidth - totalPlayersTextSize.X) / 2;
                var totalPlayersTextPosY = textPosY + textSize.Y + 20;
                float padding = 10.0f;
                Vector2 bgRectMin = new Vector2(totalPlayersTextPosX - padding, totalPlayersTextPosY - padding);
                Vector2 bgRectMax = new Vector2(totalPlayersTextPosX + totalPlayersTextSize.X + padding, totalPlayersTextPosY + totalPlayersTextSize.Y + padding);
                uint bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 0.5f));
                drawList.AddRectFilled(bgRectMin, bgRectMax, bgColor, 5.0f);
                uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 0.8f));
                drawList.AddRect(bgRectMin, bgRectMax, borderColor, 5.0f);
                drawList.AddText(new Vector2(totalPlayersTextPosX, totalPlayersTextPosY), textColor, totalPlayersText);
            }
        }

        private void DrawMinimap()
        {
            // Lógica do minimapa permanece aqui
        }

        void DrawSmallTextWithOutline(Vector2 pos, string text, uint textColor, uint outlineColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            float outlineThickness = 1.2f;
            float boldOffset = 0.4f;
            float spacing = 2.0f;
            Vector2 adjustedPos = pos;
            foreach (char c in text)
            {
                string character = c.ToString();
                for (float x = -outlineThickness; x <= outlineThickness; x += 1.0f)
                {
                    for (float y = -outlineThickness; y <= outlineThickness; y += 1.0f)
                    {
                        if (x == 0 && y == 0) continue;
                        vList.AddText(new Vector2(adjustedPos.X + x, adjustedPos.Y + y), outlineColor, character);
                    }
                }
                vList.AddText(new Vector2(adjustedPos.X - boldOffset, adjustedPos.Y), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X + boldOffset, adjustedPos.Y), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X, adjustedPos.Y - boldOffset), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X, adjustedPos.Y + boldOffset), textColor, character);
                vList.AddText(adjustedPos, textColor, character);
                adjustedPos.X += ImGui.CalcTextSize(character).X + spacing;
            }
        }

        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));
                drawList.AddLine(start, end, glowColor, thickness + (glowRadius - i) * 0.5f);
            }
            drawList.AddLine(start, end, color, thickness);
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));
                float radius = thickness / 2 + (glowRadius - i) * 0.5f;
                drawList.AddCircleFilled(start, radius, glowColor);
                drawList.AddCircleFilled(end, radius, glowColor);
            }
            drawList.AddCircleFilled(start, thickness / 2, color);
            drawList.AddCircleFilled(end, thickness / 2, color);
        }

        private float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public void DrawGradientBox(float X, float Y, float W, float H, Color topColor, Color bottomColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            int slices = 50;
            float sliceHeight = H / slices;
            for (int i = 0; i < slices; i++)
            {
                float t = (float)i / slices;
                Color sliceColor = Color.FromArgb(
                    (int)(topColor.A * (1 - t) + bottomColor.A * t),
                    (int)(topColor.R * (1 - t) + bottomColor.R * t),
                    (int)(topColor.G * (1 - t) + bottomColor.G * t),
                    (int)(topColor.B * (1 - t) + bottomColor.B * t)
                );
                uint sliceColorUint = ColorToUint32(sliceColor);
                vList.AddRectFilled(
                    new Vector2(X, Y + i * sliceHeight),
                    new Vector2(X + W, Y + (i + 1) * sliceHeight),
                    sliceColorUint
                );
            }
        }

        public void DrawFilledCircle(float centerY, float radius, int numSegments = 64)
        {
            var vList = ImGui.GetBackgroundDrawList();
            float centerX = Core.Width / 2f;
            uint colorG = ColorToUint32(Color.FromArgb((int)(1f * 255), 0, 255, 0));
            float shadowOffset = 1.08f;
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius + shadowOffset, shadowColor, numSegments);
            // Lógica do AimBot foi removida, desenha sempre o círculo verde.
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius, colorG, numSegments);
        }

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetForegroundDrawList();
            float lineW = W / 3;
            float lineH = H / 3;
            vList.AddLine(new Vector2(X, Y - thickness / 2), new Vector2(X, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X - thickness / 2, Y), new Vector2(X + lineW, Y), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W + thickness / 2, Y), color, thickness);
            vList.AddLine(new Vector2(X + W, Y - thickness / 2), new Vector2(X + W, Y + lineH), color, thickness);
            vList.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H + thickness / 2), color, thickness);
            vList.AddLine(new Vector2(X - thickness / 2, Y + H), new Vector2(X + lineW, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W + thickness / 2, Y + H), color, thickness);
            vList.AddLine(new Vector2(X + W, Y + H - lineH), new Vector2(X + W, Y + H + thickness / 2), color, thickness);
        }

        public void Draw3dBox(float X, float Y, float W, float H, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var vList = ImGui.GetForegroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);
            Vector3[] screentions = new Vector3[]
            {
                new Vector3(X, Y, 0), new Vector3(X, Y + H, 0), new Vector3(X + W, Y + H, 0), new Vector3(X + W, Y, 0),
                new Vector3(X, Y, -W), new Vector3(X, Y + H, -W), new Vector3(X + W, Y + H, -W), new Vector3(X + W, Y, -W)
            };
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));
                float currentThickness = thickness + (glowRadius - i);
                DrawBoxLinesWithGlow(vList, screentions, glowColor, currentThickness, new int[] { 0, 1, 2, 3 });
                AddGlowCircles(vList, screentions[0], currentThickness, glowColor);
                AddGlowCircles(vList, screentions[3], currentThickness, glowColor);
                DrawBoxLinesWithGlow(vList, screentions, glowColor, currentThickness, new int[] { 4, 5, 6, 7 });
                AddGlowCircles(vList, screentions[4], currentThickness, glowColor);
                AddGlowCircles(vList, screentions[7], currentThickness, glowColor);
                for (int j = 0; j < 4; j++)
                {
                    vList.AddLine(new Vector2(screentions[j].X, screentions[j].Y), new Vector2(screentions[j + 4].X, screentions[j + 4].Y), glowColor, currentThickness);
                    AddGlowCircles(vList, screentions[j], currentThickness, glowColor);
                    AddGlowCircles(vList, screentions[j + 4], currentThickness, glowColor);
                }
            }
            DrawBoxLinesWithGlow(vList, screentions, color, thickness, new int[] { 0, 1, 2, 3 });
            DrawBoxLinesWithGlow(vList, screentions, color, thickness, new int[] { 4, 5, 6, 7 });
            for (int j = 0; j < 4; j++)
            {
                vList.AddLine(new Vector2(screentions[j].X, screentions[j].Y), new Vector2(screentions[j + 4].X, screentions[j + 4].Y), color, thickness);
                AddGlowCircles(vList, screentions[j], thickness / 2, color);
                AddGlowCircles(vList, screentions[j + 4], thickness / 2, color);
            }
        }

        private void AddGlowCircles(ImDrawListPtr vList, Vector3 position, float radius, uint glowColor)
        {
            vList.AddCircleFilled(new Vector2(position.X, position.Y), radius, glowColor);
        }

        private void DrawBoxLinesWithGlow(ImDrawListPtr vList, Vector3[] points, uint color, float thickness, int[] indices)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                int start = indices[i];
                int end = indices[(i + 1) % indices.Length];
                vList.AddLine(new Vector2(points[start].X, points[start].Y), new Vector2(points[end].X, points[end].Y), color, thickness);
            }
        }

        private void DrawSkeleton(Entity entity)
        {
            var drawList = ImGui.GetForegroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor);
            uint circleColor = ColorToUint32(Color.Red);

            // Converter posições da entidade para o ecrã
            var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            var spineScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine, Core.Width, Core.Height);
            var hipScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip, Core.Width, Core.Height);
            var rightFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);
            var leftFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot, Core.Width, Core.Height);
            var leftShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftSholder, Core.Width, Core.Height);
            var rightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightSholder, Core.Width, Core.Height);
            var leftElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);

            // CORREÇÃO: Adicionando a definição da variável que faltava
            var rightElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height);

            var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var leftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);

            // Desenhar linhas do esqueleto
            DrawLine(drawList, spineScreenPos, rightShoulderScreenPos, lineColor);
            DrawLine(drawList, spineScreenPos, leftShoulderScreenPos, lineColor);
            DrawLine(drawList, spineScreenPos, hipScreenPos, lineColor);

            // CORREÇÃO: Conectando os membros corretos
            // Braço Esquerdo
            DrawLine(drawList, leftShoulderScreenPos, leftElbowScreenPos, lineColor);
            DrawLine(drawList, leftElbowScreenPos, leftWristJointScreenPos, lineColor);

            // Braço Direito
            DrawLine(drawList, rightShoulderScreenPos, rightElbowScreenPos, lineColor);
            DrawLine(drawList, rightElbowScreenPos, rightWristJointScreenPos, lineColor);

            // Pernas (simplificado)
            DrawLine(drawList, hipScreenPos, rightFootScreenPos, lineColor);
            DrawLine(drawList, hipScreenPos, leftFootScreenPos, lineColor);

            float distance = entity.Distance;
            float baseRadius = 50.0f;
            float circleRadius = baseRadius / distance;
            if (headScreenPos.X > 0 && headScreenPos.Y > 0)
            {
                drawList.AddCircle(headScreenPos, circleRadius, circleColor, 30);
            }
        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
            {
                drawList.AddLine(startPos, endPos, color, 1.5f);
            }
        }

        private void DrawHealthBar(short health, short maxHealth, float x, float y, float height, float barThickness = 6f)
        {
            var drawList = ImGui.GetForegroundDrawList();
            float healthPercentage = (float)health / maxHealth;
            float barHeight = height * healthPercentage;
            float topY = y;
            for (float i = 0; i < height; i++)
            {
                float lerpFactor = i / height;
                Color color;
                if (lerpFactor < 0.5f) { color = LerpColor(Color.LimeGreen, Color.Yellow, lerpFactor * 2); }
                else { color = LerpColor(Color.Yellow, Color.Red, (lerpFactor - 0.5f) * 2); }
                if (i >= (height - barHeight))
                {
                    drawList.AddLine(new Vector2(x - 1, topY + i), new Vector2(x + barThickness - 1, topY + i), ColorToUint32(color));
                }
            }
            float strokeThickness = 1.2f;
            drawList.AddRect(new Vector2(x - strokeThickness, y - strokeThickness), new Vector2(x + barThickness + strokeThickness, y + height + strokeThickness), ColorToUint32(Color.Black), 3f, ImDrawFlags.RoundCornersAll, strokeThickness);
        }

        private void DrawHealthBarK(short health, short maxHealth, float x, float y, float height, float barThickness = 6f)
        {
            var drawList = ImGui.GetForegroundDrawList();
            float healthPercentage = (float)health / maxHealth;
            float barHeight = height * healthPercentage;
            float topY = y;
            for (float i = 0; i < height; i++)
            {
                float lerpFactor = i / height;
                Color color;
                if (lerpFactor < 0.5f) { color = LerpColor(Color.Red, Color.Red, lerpFactor * 2); }
                else { color = LerpColor(Color.Red, Color.Red, (lerpFactor - 0.5f) * 2); }
                if (i >= (height - barHeight))
                {
                    drawList.AddLine(new Vector2(x - 1, topY + i), new Vector2(x + barThickness - 1, topY + i), ColorToUint32(color));
                }
            }
            float strokeThickness = 1.2f;
            drawList.AddRect(new Vector2(x - strokeThickness, y - strokeThickness), new Vector2(x + barThickness + strokeThickness, y + height + strokeThickness), ColorToUint32(Color.Black), 3f, ImDrawFlags.RoundCornersAll, strokeThickness);
        }

        private Color LerpColor(Color start, Color end, float amount)
        {
            return Color.FromArgb(
                (int)(start.A + (end.A - start.A) * amount),
                (int)(start.R + (end.R - start.R) * amount),
                (int)(start.G + (end.G - start.G) * amount),
                (int)(start.B + (end.B - start.B) * amount)
            );
        }

        private static T GetESPColor<T>(T defaultColor)
        {
            if (Config.RGB)
            {
                if (typeof(T) == typeof(uint)) { return (T)(object)LGBT(); }
                else if (typeof(T) == typeof(Color)) { return (T)(object)LGBTColor(); }
            }
            return defaultColor;
        }

        private static uint LGBT()
        {
            float time = (float)ImGui.GetTime();
            Color col = ColorFromHSV(time * 100 % 360, 1.0f, 1.0f);
            return ColorToUint32(col);
        }

        private static Color LGBTColor()
        {
            float time = (float)ImGui.GetTime();
            return ColorFromHSV(time * 100 % 360, 1.0f, 1.0f);
        }

        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = (float)((hue / 60) - Math.Floor(hue / 60)); float v = value * 255; float p = v * (1 - saturation); float q = v * (1 - f * saturation); float t = v * (1 - (1 - f) * saturation);
            switch (hi) { case 0: return Color.FromArgb(255, (int)v, (int)t, (int)p); case 1: return Color.FromArgb(255, (int)q, (int)v, (int)p); case 2: return Color.FromArgb(255, (int)p, (int)v, (int)t); case 3: return Color.FromArgb(255, (int)p, (int)q, (int)v); case 4: return Color.FromArgb(255, (int)t, (int)p, (int)v); default: return Color.FromArgb(255, (int)v, (int)p, (int)q); }
        }

        static uint ColorToUint32(Color color)
        {
            return ImGui.ColorConvertFloat4ToU32(new Vector4(
                (float)(color.R / 255.0),
                (float)(color.G / 255.0),
                (float)(color.B / 255.0),
                (float)(color.A / 255.0)));
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        const uint WDA_NONE = 0x00000000;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        void CreateHandle()
        {
            RECT rect;
            GetWindowRect(Core.Handle, out rect);
            int x = rect.Left;
            int y = rect.Top;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            ImGui.SetWindowSize(new Vector2((float)width, (float)height));
            ImGui.SetWindowPos(new Vector2((float)x, (float)y));
            Size = new Size(width, height);
            Position = new Point(x, y);
            Core.Width = width;
            Core.Height = height;

            // Lógica do StreamMode foi removida.
        }
    }
}
