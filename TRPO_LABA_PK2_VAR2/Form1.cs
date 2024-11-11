using System.Drawing.Imaging;

namespace TRPO_LABA_PK2_VAR2;

public partial class MainForm : Form
{
    private List<string> selectedFiles = new List<string>();
    private string outputDirectory;

    public MainForm()
    {
        InitializeComponent();
        outputDirectory = Path.Combine(Application.StartupPath, "ProcessedImages");
        Directory.CreateDirectory(outputDirectory);
    }

    private void InitializeComponent()
    {
        btnSelectFiles = new Button();
        btnProcess = new Button();
        progressBar = new ProgressBar();
        lstFiles = new ListBox();
        lblStatus = new Label();
        SuspendLayout();
        // 
        // btnSelectFiles
        // 
        btnSelectFiles.Location = new Point(12, 431);
        btnSelectFiles.Name = "btnSelectFiles";
        btnSelectFiles.Size = new Size(100, 23);
        btnSelectFiles.TabIndex = 0;
        btnSelectFiles.Text = "Select Files";
        btnSelectFiles.UseVisualStyleBackColor = true;
        btnSelectFiles.Click += BtnSelectFiles_Click;
        // 
        // btnProcess
        // 
        btnProcess.Location = new Point(118, 431);
        btnProcess.Name = "btnProcess";
        btnProcess.Size = new Size(75, 23);
        btnProcess.TabIndex = 1;
        btnProcess.Text = "Process";
        btnProcess.UseVisualStyleBackColor = true;
        btnProcess.Click += BtnProcess_Click;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(12, 460);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(663, 23);
        progressBar.TabIndex = 2;
        // 
        // lstFiles
        // 
        lstFiles.FormattingEnabled = true;
        lstFiles.ItemHeight = 17;
        lstFiles.Location = new Point(15, 13);
        lstFiles.Name = "lstFiles";
        lstFiles.Size = new Size(660, 412);
        lstFiles.TabIndex = 3;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(600, 434);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(11, 17);
        lblStatus.TabIndex = 4;
        lblStatus.Text = ".";
        // 
        // MainForm
        // 
        ClientSize = new Size(687, 496);
        Controls.Add(lblStatus);
        Controls.Add(lstFiles);
        Controls.Add(progressBar);
        Controls.Add(btnProcess);
        Controls.Add(btnSelectFiles);
        Name = "MainForm";
        ResumeLayout(false);
        PerformLayout();
    }

    private async void BtnProcess_Click(object sender, EventArgs e)
    {
        if (selectedFiles.Count == 0)
        {
            MessageBox.Show("Please select images first!");
            return;
        }

        btnProcess.Enabled = false;
        btnSelectFiles.Enabled = false;
        progressBar.Maximum = selectedFiles.Count * 3; // 3 filters per image
        progressBar.Value = 0;

        try
        {
            var processingTasks = selectedFiles.Select(filePath => ProcessImageAsync(filePath));
            await Task.WhenAll(processingTasks);

            lblStatus.Text = "Processing completed!";
            MessageBox.Show("All images have been processed successfully!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during processing: {ex.Message}");
        }
        finally
        {
            btnProcess.Enabled = true;
            btnSelectFiles.Enabled = true;
        }
    }

    private async Task ProcessImageAsync(string imagePath)
    {
        // Створюємо окремі копії зображення для кожного фільтру
        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
        var filename = Path.GetFileNameWithoutExtension(imagePath);
        var extension = Path.GetExtension(imagePath);

        try
        {
            var tasks = new List<Task>
                {
                    Task.Run(() =>
                    {
                        using (var ms = new MemoryStream(imageBytes))
                        using (var originalImage = new Bitmap(ms))
                        {
                            ApplyBlur(originalImage, filename, extension);
                        }
                    }),
                    Task.Run(() =>
                    {
                        using (var ms = new MemoryStream(imageBytes))
                        using (var originalImage = new Bitmap(ms))
                        {
                            ApplyContrast(originalImage, filename, extension);
                        }
                    }),
                    Task.Run(() =>
                    {
                        using (var ms = new MemoryStream(imageBytes))
                        using (var originalImage = new Bitmap(ms))
                        {
                            ApplyBrightness(originalImage, filename, extension);
                        }
                    })
                };

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing {Path.GetFileName(imagePath)}: {ex.Message}");
        }
    }

    private void ApplyBlur(Bitmap original, string filename, string extension)
    {
        using (var processedImage = new Bitmap(original.Width, original.Height))
        {
            using (var g = Graphics.FromImage(processedImage))
            {
                var rect = new Rectangle(0, 0, processedImage.Width, processedImage.Height);
                g.DrawImage(original, rect);
            }

            // Simple box blur implementation
            for (int x = 1; x < processedImage.Width - 1; x++)
            {
                for (int y = 1; y < processedImage.Height - 1; y++)
                {
                    var avgR = 0;
                    var avgG = 0;
                    var avgB = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            var pixel = processedImage.GetPixel(x + i, y + j);
                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;
                        }
                    }

                    avgR /= 9;
                    avgG /= 9;
                    avgB /= 9;

                    processedImage.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

            SaveImageSafely(processedImage, $"{filename}_blur{extension}");
        }
    }

    private void ApplyContrast(Bitmap original, string filename, string extension)
    {
        using (var processedImage = new Bitmap(original.Width, original.Height))
        {
            using (var g = Graphics.FromImage(processedImage))
            {
                var rect = new Rectangle(0, 0, processedImage.Width, processedImage.Height);
                g.DrawImage(original, rect);
            }

            float contrast = 1.5f; // Contrast factor

            for (int x = 0; x < processedImage.Width; x++)
            {
                for (int y = 0; y < processedImage.Height; y++)
                {
                    var pixel = processedImage.GetPixel(x, y);

                    int r = ClampColor((int)((pixel.R - 128) * contrast + 128));
                    int g = ClampColor((int)((pixel.G - 128) * contrast + 128));
                    int b = ClampColor((int)((pixel.B - 128) * contrast + 128));

                    processedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            SaveImageSafely(processedImage, $"{filename}_contrast{extension}");
        }
    }

    private void ApplyBrightness(Bitmap original, string filename, string extension)
    {
        using (var processedImage = new Bitmap(original.Width, original.Height))
        {
            using (var g = Graphics.FromImage(processedImage))
            {
                var rect = new Rectangle(0, 0, processedImage.Width, processedImage.Height);
                g.DrawImage(original, rect);
            }

            float brightness = 1.2f; // Brightness factor

            for (int x = 0; x < processedImage.Width; x++)
            {
                for (int y = 0; y < processedImage.Height; y++)
                {
                    var pixel = processedImage.GetPixel(x, y);

                    int r = ClampColor((int)(pixel.R * brightness));
                    int g = ClampColor((int)(pixel.G * brightness));
                    int b = ClampColor((int)(pixel.B * brightness));

                    processedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            SaveImageSafely(processedImage, $"{filename}_brightness{extension}");
        }
    }

    private int ClampColor(int value)
    {
        if (value > 255) return 255;
        if (value < 0) return 0;
        return value;
    }

    private void SaveImageSafely(Bitmap image, string filename)
    {
        lock (this) // Synchronize access to the output directory
        {
            var savePath = Path.Combine(outputDirectory, filename);
            image.Save(savePath, ImageFormat.Jpeg);

            this.Invoke((MethodInvoker)delegate {
                progressBar.Value++;
                lblStatus.Text = $"Processed: {progressBar.Value}/{progressBar.Maximum}";
            });
        }
    }

    private void BtnSelectFiles_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFiles = ofd.FileNames.ToList();
                lstFiles.Items.Clear();
                lstFiles.Items.AddRange(ofd.FileNames);
            }
        }
    }

    private Button btnSelectFiles;
    private Button btnProcess;
    private ProgressBar progressBar;
    private ListBox lstFiles;
    private Label lblStatus;
}
