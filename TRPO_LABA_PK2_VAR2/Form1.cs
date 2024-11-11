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
        progressBar.Location = new Point(199, 431);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(559, 23);
        progressBar.TabIndex = 2;
        // 
        // lstFiles
        // 
        lstFiles.FormattingEnabled = true;
        lstFiles.ItemHeight = 17;
        lstFiles.Location = new Point(15, 13);
        lstFiles.Name = "lstFiles";
        lstFiles.Size = new Size(743, 412);
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
        ClientSize = new Size(770, 463);
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
                            Bitmap processedImage = new BlurFilter().Apply(originalImage, filename, extension);
                            SaveImageSafely(processedImage, $"{filename}_blur{extension}");
                            processedImage.Dispose();
                        }
                    }),
                    Task.Run(() =>
                    {
                        using (var ms = new MemoryStream(imageBytes))
                        using (var originalImage = new Bitmap(ms))
                        {
                            Bitmap processedImage = new ContrastFilter().Apply(originalImage, filename, extension);
                            SaveImageSafely(processedImage, $"{filename}_contrast{extension}");
                            processedImage.Dispose();
                        }
                    }),
                    Task.Run(() =>
                    {
                        using (var ms = new MemoryStream(imageBytes))
                        using (var originalImage = new Bitmap(ms))
                        {
                            Bitmap processedImage = new BrightFilter().Apply(originalImage, filename, extension);
                            SaveImageSafely(processedImage, $"{filename}_bright{extension}");
                            processedImage.Dispose();
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
