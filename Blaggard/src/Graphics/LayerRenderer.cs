using System;
using System.Collections.Generic;
using System.Linq;

namespace Blaggard.Graphics
{
    internal struct RenderWrapper
    {
        public bool ShouldRender { get; set; }
        private Func<IBlittable> factory;
        private IBlittable blittable;

        public RenderWrapper(Func<IBlittable> factory)
        {
            this.factory = factory;
            blittable = null;
            ShouldRender = false;
        }

        public IBlittable Value => blittable ??= factory.Invoke();
        public bool HasValue => blittable != null;
    }

    public class LayerRenderer : IDisposable
    {
        private RenderWrapper[] layers;

        public LayerRenderer(List<Func<IBlittable>> blittables)
        {
            layers = blittables.Select(b => new RenderWrapper(b)).ToArray();
        }

        public void Dispose()
        {
            foreach (var l in layers.Where(l => l.HasValue))
            {
                l.Value.Dispose();
            }
        }

        public IBlittable Get(int index) => layers[index].Value;
        public void SetToRender(int index) => layers[index].ShouldRender = true;

        public void Render(Display display)
        {
            var rendered = false;
            for (int i = 0; i < layers.Length; i++)
            {
                ref var layer = ref layers[i];
                if (!layer.HasValue || !layer.ShouldRender) continue;

                var blittable = layer.Value;
                blittable.Render();
                display.Blit(blittable);

                layer.ShouldRender = false;
                rendered = true;
            }

            if (rendered)
            {
                display.Flush();
            }
        }
    }
}