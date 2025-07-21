using AotForms;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using static AotForms.WinAPI;
using static TheArtOfDevHtmlRenderer.Adapters.RGraphicsPath;

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


            if (Config.Aimfovc)
            {
                // Draw a single glow layer
                uint glowColor = ColorToUint32(Color.FromArgb((int)(1f * 255), Config.Aimfovcolor));

                DrawSmoothCircle(Config.AimBotFov, glowColor, 1.0f);
            }

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

            // DrawFilledCircle(sharedCirclePosition, 5.0f);

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

                if (dist > Config.espran) continue;

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

                    // ---- SLOWER ROTATION CROSSHAIR ----
                    float rotationSpeed = 0.3f;  // Slower rotation
                    float angle = (float)(Environment.TickCount * rotationSpeed % 360);
                    float angleRad = MathF.PI * angle / 180f;

                    float crosshairLength = 12f;

                    // ---- SLOWER COLOR TRANSITION ----
                    float time = Environment.TickCount * 0.002f; // Slower color change
                    Color rainbowColor = ColorFromHSV((time * 100) % 360, 1.0f, 1.0f);
                    uint rainbowColorUint = ColorToUint32(rainbowColor);

                    Vector2 crosshairTop = new Vector2(Core.Width / 2f + crosshairLength * MathF.Cos(angleRad), Core.Height / 2f - crosshairLength * MathF.Sin(angleRad));
                    Vector2 crosshairBottom = new Vector2(Core.Width / 2f - crosshairLength * MathF.Cos(angleRad), Core.Height / 2f + crosshairLength * MathF.Sin(angleRad));
                    Vector2 crosshairLeft = new Vector2(Core.Width / 2f - crosshairLength * MathF.Sin(angleRad), Core.Height / 2f - crosshairLength * MathF.Cos(angleRad));
                    Vector2 crosshairRight = new Vector2(Core.Width / 2f + crosshairLength * MathF.Sin(angleRad), Core.Height / 2f + crosshairLength * MathF.Cos(angleRad));

                    // ---- DRAW ROTATING RGB CROSSHAIR ----
                    drawList.AddLine(crosshairTop, crosshairBottom, rainbowColorUint, 2.5f);
                    drawList.AddLine(crosshairLeft, crosshairRight, rainbowColorUint, 2.5f);

                    // ---- GARENA CHEATS TEXT (BOLD, OUTLINED, RGB) ----
                    string displayText = "ANIK X CHEATS";
                    
                    Vector2 textPosition = new Vector2(Core.Width / 2f - 100f, Core.Height - 50f);

                    uint outlineColor = ColorToUint32(Color.Black);

                    // Use DrawSmallTextWithOutline to make text bold with outline
                    DrawSmallTextWithOutline(textPosition, displayText, rainbowColorUint, outlineColor);
                }

                // ---- FUNCTION TO GENERATE SMOOTH RAINBOW COLORS ----
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

                    DrawGradientBox(
                        headScreenPos.X - (CornerWidth / 2),
                        headScreenPos.Y,
                        CornerWidth,
                        CornerHeight,
                        topColor,
                        bottomColor
                    );
                }


                if (Config.ESPBox2)
                {
                    uint boxColor = GetESPColor(ColorToUint32(Config.ESPBoxColor));

                    // Define glow parameters for 3D box
                    float glowRadius = 15f; // Adjust the glow radius as needed
                    float feather = 1.7f; // Feather effect for the glow
                    float glowOpacityMultiplier = 0.02f; // Glow opacity control

                    Draw3dBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f, Config.BoxGlow ? 1f : 0f, feather, glowOpacityMultiplier);
                }

                if (Config.ESPBox)
                {
                    uint boxColor = GetESPColor(ColorToUint32(Config.ESPBoxColor));

                    DrawCorneredBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);
                }

                var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "Bot" : entity.Name;
                var namePosition = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 28);
                var nameSize = ImGui.CalcTextSize($"{MathF.Round(dist)}M" + nameText);

                if (Config.ESPName)
                {
                    // ReplaceFont(fontpath1, 15, FontGlyphRangeType.English);
                    DrawSmallTextWithOutline(namePosition, nameText, GetESPColor(ColorToUint32(Color.White)), ColorToUint32(Color.Black));
                }
                if (Config.ESPDistance)
                {
                    //  ImGui.PushFont(_fontPtr2); // Usar a fonte personalizada
                    // Calculate the distance string
                    string distanceText = $"{MathF.Round(dist)} M";

                    // Estimate text width (assuming an average width of 8 pixels per character)
                    float estimatedTextWidth = distanceText.Length * 8f;

                    // Adjust the position for the distance text, centering it
                    Vector2 distancePosition = new Vector2(bottomScreenPos.X + 1 - (estimatedTextWidth / 2), bottomScreenPos.Y + 5f);

                    // Draw the distance in yellow

                    DrawSmallTextWithOutline(distancePosition, distanceText, GetESPColor(ColorToUint32(Config.ESPSkeletonColor)), ColorToUint32(Color.Black));
                    //  ImGui.PopFont(); // Voltar para a fonte padrÃ£o


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
                // Display the total enemy count
                string totalPlayersText = $"Enemy Detected: {enemyCount}";
                var totalPlayersTextSize = ImGui.CalcTextSize(totalPlayersText);
                var totalPlayersTextPosX = (windowWidth - totalPlayersTextSize.X) / 2;
                var totalPlayersTextPosY = textPosY + textSize.Y + 20;


                // Define padding for the background
                float padding = 10.0f;

                // Calculate the background rectangle position and size
                Vector2 bgRectMin = new Vector2(totalPlayersTextPosX - padding, totalPlayersTextPosY - padding);
                Vector2 bgRectMax = new Vector2(totalPlayersTextPosX + totalPlayersTextSize.X + padding, totalPlayersTextPosY + totalPlayersTextSize.Y + padding);

                // Draw the transparent background
                uint bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 0.5f)); // Semi-transparent black
                drawList.AddRectFilled(bgRectMin, bgRectMax, bgColor, 5.0f); // Rounded corners with a radius of 5.0f
                uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 0.8f)); // Semi-transparent white
                drawList.AddRect(bgRectMin, bgRectMax, borderColor, 5.0f); // Rounded corners with a radius of 5.0f
                                                                           // Draw the main "Total Players" text on top
                drawList.AddText(new Vector2(totalPlayersTextPosX, totalPlayersTextPosY), textColor, totalPlayersText);

            }
        }

        private void DrawMinimap()
        {
            var windowWidth = Core.Width;
            var windowHeight = Core.Height;

            int DetectionRange = 250;

            float minimapSize = 180f * (DetectionRange / 250f); // Uniform circular minimap
            Vector2 minimapCenter = new Vector2(100, windowHeight - 100);

            float cameraYaw = -GetCameraYaw();
            float cosYaw = MathF.Cos(cameraYaw);
            float sinYaw = MathF.Sin(cameraYaw);

            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
            uint minimapBackgroundColor = ColorToUint32(Color.FromArgb(180, 20, 20, 20));
            uint minimapBorderColor = ColorToUint32(Color.FromArgb(255, 255, 255, 255));

            // Smooth circular minimap
            drawList.AddCircleFilled(minimapCenter, minimapSize / 2, minimapBackgroundColor);
            drawList.AddCircle(minimapCenter, minimapSize / 2 + 2, minimapBorderColor, 100, 2.0f); // Border

            // Grid lines (circular effect)
            uint gridColor = ColorToUint32(Color.FromArgb(80, 200, 200, 200));
            for (float i = 0.25f; i <= 1f; i += 0.25f)
            {
                drawList.AddCircle(minimapCenter, (minimapSize / 2) * i, gridColor, 100, 0.5f);
            }

            // Compass directions with better visibility
            string[] compassDirections = { "N", "E", "S", "W" };
            for (int i = 0; i < 4; i++)
            {
                float angle = cameraYaw + i * MathF.PI / 2;
                Vector2 directionPos = minimapCenter + new Vector2(MathF.Cos(angle), -MathF.Sin(angle)) * (minimapSize / 2 - 10);
                drawList.AddText(directionPos - new Vector2(5, 5), ColorToUint32(Color.White), compassDirections[i]);
            }

            // Player indicator (triangle)
            uint playerColor = ColorToUint32(Color.Cyan);
            Vector2[] playerTriangle = new Vector2[3]
            {
        minimapCenter + new Vector2(0, -8),   // Top
        minimapCenter + new Vector2(-6, 6),  // Left
        minimapCenter + new Vector2(6, 6)    // Right
            };

            for (int i = 0; i < 3; i++) // Rotate based on cameraYaw
            {
                float x = playerTriangle[i].X - minimapCenter.X;
                float y = playerTriangle[i].Y - minimapCenter.Y;
                playerTriangle[i].X = minimapCenter.X + (x * cosYaw - y * sinYaw);
                playerTriangle[i].Y = minimapCenter.Y + (x * sinYaw + y * cosYaw);
            }

            drawList.AddTriangleFilled(playerTriangle[0], playerTriangle[1], playerTriangle[2], playerColor);

            // Draw entities
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead) continue;

                float distance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (distance > DetectionRange) continue;

                Vector3 relativePosition = entity.Head - Core.LocalMainCamera;
                float scale = minimapSize / (float)DetectionRange;

                float rotatedX = relativePosition.X * cosYaw - relativePosition.Z * sinYaw;
                float rotatedY = relativePosition.X * sinYaw + relativePosition.Z * cosYaw;

                Vector2 enemyOnMinimap = minimapCenter + new Vector2(rotatedX * scale, -rotatedY * scale);

                if (Vector2.Distance(enemyOnMinimap, minimapCenter) <= minimapSize / 2)
                {
                    uint enemyColor = entity.IsKnown
                        ? (entity.IsKnocked ? ColorToUint32(Color.Yellow) : ColorToUint32(Color.Red))
                        : ColorToUint32(Color.Blue);

                    // Dynamic glow effect for better visibility
                    drawList.AddCircleFilled(enemyOnMinimap + new Vector2(2, 2), 6.0f, ColorToUint32(Color.FromArgb(100, 0, 0, 0))); // Shadow
                    drawList.AddCircleFilled(enemyOnMinimap, 5.0f, enemyColor);
                }
            }
        }



        private float GetCameraYaw()
        {
            return MathF.Atan2(Core.CameraMatrix.M31, Core.CameraMatrix.M33);
        }

        void DrawSmallTextWithOutline(Vector2 pos, string text, uint textColor, uint outlineColor)
        {
            var vList = ImGui.GetForegroundDrawList();
            float outlineThickness = 1.2f;  // Smaller outline for smoothness
            float boldOffset = 0.4f;        // Adjusted for smaller text
            float spacing = 2.0f;           // Adds space between characters

            Vector2 adjustedPos = pos;

            foreach (char c in text)
            {
                string character = c.ToString(); // Convert char to string

                // Smooth outline
                for (float x = -outlineThickness; x <= outlineThickness; x += 1.0f)
                {
                    for (float y = -outlineThickness; y <= outlineThickness; y += 1.0f)
                    {
                        if (x == 0 && y == 0) continue;
                        vList.AddText(new Vector2(adjustedPos.X + x, adjustedPos.Y + y), outlineColor, character);
                    }
                }

                // Bold effect
                vList.AddText(new Vector2(adjustedPos.X - boldOffset, adjustedPos.Y), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X + boldOffset, adjustedPos.Y), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X, adjustedPos.Y - boldOffset), textColor, character);
                vList.AddText(new Vector2(adjustedPos.X, adjustedPos.Y + boldOffset), textColor, character);

                // Main text layer
                vList.AddText(adjustedPos, textColor, character);

                // Move position for next character with spacing
                adjustedPos.X += ImGui.CalcTextSize(character).X + spacing;
            }
        }





        public void DrawGlowLine(Vector2 start, Vector2 end, uint color, float thickness, float glowRadius, float feather, float glowOpacityMultiplier)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(color);

            // Outer glow layers for the line
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);

                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));

                // Draw the glow for the line
                drawList.AddLine(start, end, glowColor, thickness + (glowRadius - i) * 0.5f);
            }

            // Main line at the center
            drawList.AddLine(start, end, color, thickness);

            // Draw start and end glows as circles
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);

                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));

                // Draw circles at the start and end points
                float radius = thickness / 2 + (glowRadius - i) * 0.5f;
                drawList.AddCircleFilled(start, radius, glowColor);
                drawList.AddCircleFilled(end, radius, glowColor);
            }

            // Draw rounded corners for the main line
            drawList.AddCircleFilled(start, thickness / 2, color);
            drawList.AddCircleFilled(end, thickness / 2, color);
        }

        // Custom clamp function
        private float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }




        public void DrawGradientBox(float X, float Y, float W, float H, Color topColor, Color bottomColor)
        {
            var vList = ImGui.GetForegroundDrawList();

            int slices = 50; // Number of slices for gradient
            float sliceHeight = H / slices;

            for (int i = 0; i < slices; i++)
            {
                float t = (float)i / slices; // Interpolation factor
                Color sliceColor = Color.FromArgb(
                    (int)(topColor.A * (1 - t) + bottomColor.A * t), // Interpolating opacity
                    (int)(topColor.R * (1 - t) + bottomColor.R * t), // Interpolating Red
                    (int)(topColor.G * (1 - t) + bottomColor.G * t), // Interpolating Green
                    (int)(topColor.B * (1 - t) + bottomColor.B * t)  // Interpolating Blue
                );

                uint sliceColorUint = ColorToUint32(sliceColor);

                // Draw each slice
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

            // Set the center of the circle at the middle of the screen horizontally (Core.Width / 2f)

            float centerX = Core.Width / 2f;

            uint colorR = ColorToUint32(Color.FromArgb((int)(1f * 255), 225, 0, 0)); // Red color with full opacity
            uint colorG = ColorToUint32(Color.FromArgb((int)(1f * 255), 0, 255, 0)); // LimeGreen color with full opacity

            // Shadow parameters
            float shadowOffset = 1.08f; // The subtle offset of the shadow from the circle
            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f)); // Semi-transparent black for a soft shadow

            // Draw shadow (a larger circle slightly offset behind the main one)
            vList.AddCircleFilled(new Vector2(centerX, centerY), radius + shadowOffset, shadowColor, numSegments);

            if (Config.AimBot)
            {
                // Draw main circle
                vList.AddCircleFilled(new Vector2(centerX, centerY), radius, colorR, numSegments);
            }
            else
            {
                // Draw main circle
                vList.AddCircleFilled(new Vector2(centerX, centerY), radius, colorG, numSegments);
            }
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
        new Vector3(X, Y, 0),
        new Vector3(X, Y + H, 0),
        new Vector3(X + W, Y + H, 0),
        new Vector3(X + W, Y, 0),
        new Vector3(X, Y, -W),
        new Vector3(X, Y + H, -W),
        new Vector3(X + W, Y + H, -W),
        new Vector3(X + W, Y, -W)
            };

            // Draw glow effect for each line and add circles at start and end
            for (float i = glowRadius; i > 0; i -= feather)
            {
                float alpha = colorVec.W * (i / glowRadius) * glowOpacityMultiplier;
                alpha = Clamp(alpha, 0, 1);
                uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(colorVec.X, colorVec.Y, colorVec.Z, alpha));

                float currentThickness = thickness + (glowRadius - i);

                // Front face with circle glow at start and end
                DrawBoxLinesWithGlow(vList, screentions, glowColor, currentThickness, new int[] { 0, 1, 2, 3 });
                AddGlowCircles(vList, screentions[0], currentThickness, glowColor);
                AddGlowCircles(vList, screentions[3], currentThickness, glowColor);

                // Back face with circle glow at start and end
                DrawBoxLinesWithGlow(vList, screentions, glowColor, currentThickness, new int[] { 4, 5, 6, 7 });
                AddGlowCircles(vList, screentions[4], currentThickness, glowColor);
                AddGlowCircles(vList, screentions[7], currentThickness, glowColor);

                // Connecting lines with circle glow at start and end
                for (int j = 0; j < 4; j++)
                {
                    vList.AddLine(new Vector2(screentions[j].X, screentions[j].Y), new Vector2(screentions[j + 4].X, screentions[j + 4].Y), glowColor, currentThickness);
                    AddGlowCircles(vList, screentions[j], currentThickness, glowColor);
                    AddGlowCircles(vList, screentions[j + 4], currentThickness, glowColor);
                }
            }

            // Main box (without glow) 
            DrawBoxLinesWithGlow(vList, screentions, color, thickness, new int[] { 0, 1, 2, 3 }); // Front face
            DrawBoxLinesWithGlow(vList, screentions, color, thickness, new int[] { 4, 5, 6, 7 }); // Back face

            for (int j = 0; j < 4; j++)
            {
                vList.AddLine(new Vector2(screentions[j].X, screentions[j].Y), new Vector2(screentions[j + 4].X, screentions[j + 4].Y), color, thickness);
                AddGlowCircles(vList, screentions[j], thickness / 2, color);
                AddGlowCircles(vList, screentions[j + 4], thickness / 2, color);
            }
        }

        // Helper method to add glow circles at the start and end of lines
        private void AddGlowCircles(ImDrawListPtr vList, Vector3 position, float radius, uint glowColor)
        {
            vList.AddCircleFilled(new Vector2(position.X, position.Y), radius, glowColor);
        }


        // Helper method to draw lines for a box face with glow
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
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor); // Color for the skeleton lines
            uint circleColor = ColorToUint32(Color.Red); // Color for the circle around the head

            // Convert entity positions to screen space
            var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            var leftWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height); // Adjust as per actual mapping
            var spineScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine, Core.Width, Core.Height);
            var hipScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip, Core.Width, Core.Height); // Adjust as per actual mapping
            var rootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);
            var rightCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightCalf, Core.Width, Core.Height);
            var leftCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftCalf, Core.Width, Core.Height);
            var rightFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);
            var leftFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot, Core.Width, Core.Height);
            var rightWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height);
            var leftHandScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftHand, Core.Width, Core.Height);
            var leftShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftSholder, Core.Width, Core.Height);
            var rightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightSholder, Core.Width, Core.Height);
            var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var leftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);
            var leftElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);
            var rightElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height); // Adjust if needed

            // Draw skeleton lines


            DrawLine(drawList, spineScreenPos, rightShoulderScreenPos, lineColor); // Spine to Right Shoulder
            DrawLine(drawList, spineScreenPos, hipScreenPos, lineColor);// Spine to hip


            DrawLine(drawList, spineScreenPos, leftShoulderScreenPos, lineColor); // Spine to Left Shoulder
            DrawLine(drawList, leftShoulderScreenPos, rightElbowScreenPos, lineColor); // Left Shoulder to Left Elbow
            DrawLine(drawList, leftElbowScreenPos, rightWristJointScreenPos, lineColor); // Left Elbow to Left Wrist Joint
            // Left Wrist Joint to Left Wrist

            DrawLine(drawList, rightShoulderScreenPos, leftElbowScreenPos, lineColor); // Right Shoulder to Left Elbow
                                                                                       //  DrawLine(drawList, rightElbowScreenPos, leftWristJointScreenPos, lineColor); // Right Elbow to Left Wrist Joint
                                                                                       // Right Wrist Joint to Left Wrist

            DrawLine(drawList, hipScreenPos, rightFootScreenPos, lineColor);// Hip to Right Calf
            DrawLine(drawList, hipScreenPos, leftFootScreenPos, lineColor);// Hip to Left Calf


            // Draw a small circle around the head
            float distance = entity.Distance; // Assume entity.Distance is the distance to the player in game units

            // Calculate the circle radius based on distance (e.g., closer = larger, farther = smaller)
            float baseRadius = 50.0f; // Adjust this base value as needed
            float circleRadius = baseRadius / distance;

            // Draw the circle on the head if the head is visible on screen
            if (headScreenPos.X > 0 && headScreenPos.Y > 0)
            {
                drawList.AddCircle(headScreenPos, circleRadius, circleColor, 30); // 30 segments for the circle
            }

            // Add additional code here to draw the rest of the skeleton using the updated bone positions
        }
        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)
        {
            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)
            {
                drawList.AddLine(startPos, endPos, color, 1.5f); // Adjust thickness as needed
            }
        }

        public void DrawSmoothCircle(float radius, uint color, float thickness, int segments = 64)
        {
            var vList = ImGui.GetForegroundDrawList();
            var io = ImGui.GetIO();
            float centerX = io.DisplaySize.X / 2;
            float centerY = io.DisplaySize.Y / 2;

            vList.AddCircle(new Vector2(centerX, centerY), radius, color, segments, thickness);
        }



        // Helper function for linear interpolation of colors




        private void DrawHealthBar(short health, short maxHealth, float x, float y, float height, float barThickness = 6f)
        {
            var drawList = ImGui.GetForegroundDrawList();
            float healthPercentage = (float)health / maxHealth;
            float barHeight = height * healthPercentage;

            // Calculate the fixed gradient positions
            float topY = y;
            float bottomY = y + height;



            // Position offsets
            float offsetX = -1; // Move right by 50 units
            float offsetY = 0; // Move down by 30 units

            // Draw Gradient Health Bar
            for (float i = 0; i < height; i++)
            {
                float lerpFactor = i / height; // Linear interpolation factor (0.0 to 1.0 across the full bar height)

                // Interpolate between three colors (top: LimeGreen, middle: Yellow, bottom: Red)
                Color color;
                if (lerpFactor < 0.5f)
                {
                    color = LerpColor(Color.LimeGreen, Color.Yellow, lerpFactor * 2);
                }
                else
                {
                    color = LerpColor(Color.Yellow, Color.Red, (lerpFactor - 0.5f) * 2);
                }

                // Only draw the portion of the gradient visible for the current health
                if (i >= (height - barHeight))
                {
                    drawList.AddLine(
                        new Vector2(x + offsetX, topY + offsetY + i),
                        new Vector2(x + barThickness + offsetX, topY + offsetY + i),
                        ColorToUint32(color)
                    );
                }
            }

            // Draw Stroke (outline)
            float strokeThickness = 1.2f; // Adjust stroke thickness as needed
            drawList.AddRect(new Vector2(x - strokeThickness, y - strokeThickness),
                             new Vector2(x + barThickness + strokeThickness, y + height + strokeThickness),
                             ColorToUint32(Color.Black), // Stroke color
                             3f, // Corner rounding radius
                             ImDrawFlags.RoundCornersAll,
                             strokeThickness);
        }



        private void DrawHealthBarK(short health, short maxHealth, float x, float y, float height, float barThickness = 6f)
        {
            var drawList = ImGui.GetForegroundDrawList();
            float healthPercentage = (float)health / maxHealth;
            float barHeight = height * healthPercentage;

            // Calculate the fixed gradient positions
            float topY = y;
            float bottomY = y + height;



            // Position offsets
            float offsetX = -1; // Move right by 50 units
            float offsetY = 0; // Move down by 30 units

            // Draw Gradient Health Bar
            for (float i = 0; i < height; i++)
            {
                float lerpFactor = i / height; // Linear interpolation factor (0.0 to 1.0 across the full bar height)

                // Interpolate between three colors (top: LimeGreen, middle: Yellow, bottom: Red)
                Color color;
                if (lerpFactor < 0.5f)
                {
                    color = LerpColor(Color.Red, Color.Red, lerpFactor * 2);
                }
                else
                {
                    color = LerpColor(Color.Red, Color.Red, (lerpFactor - 0.5f) * 2);
                }

                // Only draw the portion of the gradient visible for the current health
                if (i >= (height - barHeight))
                {
                    drawList.AddLine(
                        new Vector2(x + offsetX, topY + offsetY + i),
                        new Vector2(x + barThickness + offsetX, topY + offsetY + i),
                        ColorToUint32(color)
                    );
                }
            }

            // Draw Stroke (outline)
            float strokeThickness = 1.2f; // Adjust stroke thickness as needed
            drawList.AddRect(new Vector2(x - strokeThickness, y - strokeThickness),
                             new Vector2(x + barThickness + strokeThickness, y + height + strokeThickness),
                             ColorToUint32(Color.Black), // Stroke color
                             3f, // Corner rounding radius
                             ImDrawFlags.RoundCornersAll,
                             strokeThickness);
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
                if (typeof(T) == typeof(uint))
                {
                    return (T)(object)LGBT(); // Return uint when needed
                }
                else if (typeof(T) == typeof(Color))
                {
                    return (T)(object)LGBTColor(); // Return Color when needed
                }
            }
            return defaultColor;
        }

        private static uint LGBT()
        {
            float time = (float)ImGui.GetTime();
            Color col = ColorFromHSV(time * 100 % 360, 1.0f, 1.0f);
            return ColorToUint32(col); // Convert RGB color to Uint32
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
        const uint WDA_MONITOR = 0x00000001;
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
            // LoadCustomFont();
            if (Config.StreamMode)
            {
                SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE);
            }
            else
            {
                SetWindowDisplayAffinity(hWnd, WDA_NONE);
            }

        }
    }
}
