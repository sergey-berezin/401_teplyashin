using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace ClassLibrary2
{
    public class Class1
    {
        private InferenceSession? session = null;
        public Class1()
        {
            using var modelStream = typeof(Class1).Assembly.GetManifestResourceStream("ClassLibrary2.emotion-ferplus-7.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);
            session = new InferenceSession(memoryStream.ToArray());
        }
        public async Task<(string, float)[]> process(string path, CancellationToken token)
        {

            return await Task<(string, float)[]>.Factory.StartNew(() => {
                string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
                var res = new (string, float)[keys.Length];
                using Image<Rgb24> image = Image.Load<Rgb24>(path);
                image.Mutate(ctx => {
                    ctx.Resize(new Size(64, 64));
                });
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
                var emotions = Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());
                for (int i = 0; i < keys.Length; i++)
                {
                    if (token.IsCancellationRequested)
                        break;
                    else
                        res[i] = (keys[i], emotions[i]);
                }
                return res;
            }, token);

        }
        DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 1, h, w });

            img.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R; // B and G are the same
                    }
                }
            });

            return t;
        }

        float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }
    }
}
