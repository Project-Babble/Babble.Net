using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Drawing;

class Program
{
    static void Main()
    {
        // Path to the ONNX model
        string modelPath = "model.onnx";

        // Load the image from file
        string imagePath = "0085.png";
        var image = new Bitmap(imagePath)!;

        // Load and preprocess the image
        var inputTensor = LoadImageAndPreprocess(imagePath);

        //var sessionOptions = new SessionOptions();
        //sessionOptions.AppendExecutionProvider_CUDA(1);

        // Load ONNX model and create session
        using var session = new InferenceSession(modelPath);

        // Create input container
        var inputName = session.InputMetadata.Keys.First();
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) };

        // Run inference
        using var results = session.Run(inputs);

        // Get output and display
        var output = results[0].AsEnumerable<float>().ToArray();
        Console.WriteLine("Model Output:");
        foreach (var value in output)
        {
            Console.WriteLine(value);
        }
    }

    static Tensor<float> LoadImageAndPreprocess(string imagePath)
    {
        // Load the image
        using var bitmap = new Bitmap(imagePath);

        // Convert to grayscale, resize, and normalize (1/255.0)
        var resized = new Bitmap(bitmap, new Size(256, 256));
        var input = new DenseTensor<float>([1, 1, 256, 256]);

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                // Grayscale and normalize (assuming RGB input)
                var pixel = resized.GetPixel(x, y);
                var grayscale = (float)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B) / 255.0f;
                input[0, 0, y, x] = grayscale;
            }
        }

        return input;
    }
}
