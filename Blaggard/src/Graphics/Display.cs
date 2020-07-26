using System;
using System.Collections.Generic;

using SDL2;
using Blaggard.Common;

namespace Blaggard.Graphics
{
    // TODO: Implement recreating display with new resolution via config
    public class Display : IDisposable
    {
        private IntPtr window, renderer, fontTexture, glyphTexture, rootTexture;
        private int width, height, xOffset, yOffset, cellWidth, cellHeight;

        private int mult;

        private bool fullscreen = false;
        private bool dirty;
        private bool terminalMode = false;

        public int Width => width;
        public int Height => height;

        public int CellWidth => cellWidth * mult;
        public int CellHeight => cellHeight * mult;

        public Display(int xResolution, int yResolution, int pixelMult, bool terminalMode)
        {
            Console.WriteLine("Initializing graphics subsystem.");

            this.terminalMode = terminalMode;

            mult = pixelMult;

            cellWidth = 8;
            cellHeight = 8;

            width = xResolution;
            height = yResolution;

            xOffset = (xResolution * CellWidth - width * CellWidth) / 2;
            yOffset = (yResolution * CellHeight - height * CellHeight) / 2;

            window = SDL.SDL_CreateWindow("Waste Drudgers", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, xResolution * CellWidth, yResolution * CellHeight, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);

            fontTexture = SDL_image.IMG_LoadTexture(renderer, "assets/font8x8.png");
            glyphTexture = SDL_image.IMG_LoadTexture(renderer, "assets/glyph8x8.png");
            rootTexture = CreateTexture(width * CellWidth, height * CellHeight);

            SDL.SDL_RenderClear(renderer);
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing Graphics subsystem.");

            FreeTexture(fontTexture);
            FreeTexture(glyphTexture);
            FreeTexture(rootTexture);

            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
        }

        public IntPtr CreateTexture(int pixelsWidth, int pixelsHeight) => SDL.SDL_CreateTexture(
            renderer,
            SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            pixelsWidth,
            pixelsHeight
        );

        public IntPtr GetRenderer() => renderer;

        public void FreeTexture(IntPtr texture) => SDL.SDL_DestroyTexture(texture);

        public void Clear(int x, int y, int width, int height, Color background)
        {
            SDL.SDL_SetRenderTarget(renderer, rootTexture);

            var dstRect = new SDL.SDL_Rect() { x = x * CellWidth, y = y * CellHeight, w = width * CellWidth, h = height * CellHeight };

            SDL.SDL_SetRenderDrawColor(renderer, background.r, background.g, background.b, 255);
            SDL.SDL_RenderFillRect(renderer, ref dstRect);
            dirty = true;
        }

        public void ToggleFullscreen()
        {
            var flag = fullscreen ? SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL : SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            SDL.SDL_SetWindowFullscreen(window, (uint)flag);
            fullscreen = !fullscreen;
        }

        public void SetRenderTarget(IntPtr texture) => SDL.SDL_SetRenderTarget(renderer, texture);

        public void DrawCell(int x, int y, char ch, Color foreground, Color background)
        {
            bool glyph = false;
            int character;
            if (ch > 255 && ch < 512)
            {
                glyph = true;
                character = ch - 256;
            }
            else
            {
                character = CP437Utils.FromChar(ch);
            }

            int cy = character / 16;
            int cx = character - cy * 16;

            var srcRect = new SDL.SDL_Rect() { x = cx * cellWidth, y = cy * cellHeight, w = cellWidth, h = cellHeight };
            var dstRect = new SDL.SDL_Rect() { x = (x * CellWidth) + xOffset, y = (y * CellHeight) + yOffset, w = CellWidth, h = CellHeight };

            DrawCellBackground(terminalMode ? Color.black : background, ref dstRect);

            var srcTexture = glyph ? glyphTexture : fontTexture;
            SDL.SDL_SetTextureBlendMode(srcTexture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL.SDL_SetTextureColorMod(srcTexture, foreground.r, foreground.g, foreground.b);

            SDL.SDL_RenderCopy(renderer, srcTexture, ref srcRect, ref dstRect);
        }

        private void DrawCellBackground(Color background, ref SDL.SDL_Rect dstRect)
        {
            SDL.SDL_SetRenderDrawColor(renderer, background.r, background.g, background.b, 255);
            SDL.SDL_RenderFillRect(renderer, ref dstRect);
        }

        public void DrawSprite(Sprite sprite)
        {
            int h = sprite.Texture.height;

            int x = sprite.x;
            int y = sprite.y;

            var srcRect = new SDL.SDL_Rect() { x = 0, y = 0, w = 24, h = h };
            var dstRect = new SDL.SDL_Rect() { x = x, y = y, w = 24, h = h };

            SDL.SDL_RenderCopy(renderer, sprite.Texture.Value, ref srcRect, ref dstRect);
        }

        public void ClearTexture()
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL.SDL_RenderClear(renderer);
        }

        // This is used only for generating tilesets in Waste Drudgers, should probably be it's own library altogether
        public void SaveTextureToFile(IntPtr texture)
        {
            var target = SDL.SDL_GetRenderTarget(renderer);
            SDL.SDL_SetRenderTarget(renderer, texture);
            SDL.SDL_QueryTexture(texture, out var format, out var access, out var width, out var height);
            var pixels = System.Runtime.InteropServices.Marshal.AllocHGlobal(width * height * SDL.SDL_BYTESPERPIXEL(format));
            var rect = new SDL.SDL_Rect() { x = 0, y = 0, w = width, h = height };
            SDL.SDL_RenderReadPixels(renderer, ref rect, format, pixels, width * SDL.SDL_BYTESPERPIXEL(format));
            var surface = SDL.SDL_CreateRGBSurfaceWithFormatFrom(pixels, width, height, 24, width * SDL.SDL_BYTESPERPIXEL(format), format);
            SDL_image.IMG_SavePNG(surface, "./editor/tileset.png");
            SDL.SDL_FreeSurface(surface);
            SDL.SDL_SetRenderTarget(renderer, target);
        }

        /// <summary>
        /// Blit an off-screen canvas to the root texture.
        /// </summary>
        public void Blit(IBlittable blittable)
        {
            var dstRect = blittable.RenderRect.ConvertToSDLRect();
            SDL.SDL_SetRenderTarget(renderer, rootTexture);
            SDL.SDL_RenderCopy(renderer, blittable.Texture, IntPtr.Zero, ref dstRect);
            dirty = true;
        }

        /// <summary>
        /// Flush the root canvas to the renderer.
        /// </summary>
        public void Flush()
        {
            // Flush only if something was actually drawn to texture since last call
            if (dirty)
            {
                SDL.SDL_SetRenderTarget(renderer, IntPtr.Zero);
                SDL.SDL_RenderCopy(renderer, rootTexture, IntPtr.Zero, IntPtr.Zero);
                SDL.SDL_RenderPresent(renderer);
                dirty = false;
            }
        }
    }
}