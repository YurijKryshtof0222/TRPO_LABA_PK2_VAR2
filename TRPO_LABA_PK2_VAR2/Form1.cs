using System.Drawing.Imaging;

namespace TRPO_LABA_PK2_VAR2;

public partial class MainForm : Form
{
    private Button btnSelectFiles;
    private Button btnProcess;
    private ProgressBar progressBar;
    private ListBox lstFiles;
    private Label lblStatus;

    private List<string> selectedFiles = new List<string>();
    private string outputDirectory;
    private object lockObject = new object();

    public MainForm()
    {
        InitializeComponent();
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

    private void BtnProcess_Click(object sender, EventArgs e)
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

        outputDirectory = SelectDirectory();

        // Запускаємо обробку в окремому потоці
        Task.Run(() =>
        {
            try
            {
                var processingTasks = selectedFiles.Select(filePath =>
                    Task.Run(() => ProcessImage(filePath))
                ).ToArray();

                Task.WaitAll(processingTasks);

                // Оновлюємо UI після завершення всіх завдань
                this.Invoke((MethodInvoker)delegate
                {
                    lblStatus.Text = "Processing completed!";
                    MessageBox.Show("All images have been processed successfully!");
                    btnProcess.Enabled = true;
                    btnSelectFiles.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Error during processing: {ex.Message}");
                    btnProcess.Enabled = true;
                    btnSelectFiles.Enabled = true;
                });
            }
        });
    }

    private void ProcessImage(string imagePath)
    {
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        var filename = Path.GetFileNameWithoutExtension(imagePath);
        var extension = Path.GetExtension(imagePath);

        try
        {
            var tasks = new List<Task>();

            // Створюємо завдання для кожного фільтру
            tasks.Add(Task.Run(() =>
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var originalImage = new Bitmap(ms))
                {
                    Bitmap processedImage = new BlurFilter().Apply(originalImage, filename, extension);
                    SaveImageSafely(processedImage, $"{filename}_blur{extension}");
                    processedImage.Dispose();
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var originalImage = new Bitmap(ms))
                {
                    Bitmap processedImage = new ContrastFilter().Apply(originalImage, filename, extension);
                    SaveImageSafely(processedImage, $"{filename}_contrast{extension}");
                    processedImage.Dispose();
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var originalImage = new Bitmap(ms))
                {
                    Bitmap processedImage = new BrightFilter().Apply(originalImage, filename, extension);
                    SaveImageSafely(processedImage, $"{filename}_bright{extension}");
                    processedImage.Dispose();
                }
            }));

            Task.WaitAll(tasks.ToArray());
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing {Path.GetFileName(imagePath)}: {ex.Message}");
        }
    }

    private string SelectDirectory()
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                return fbd.SelectedPath;
            }

            string defaultPath = Path.Combine(Application.StartupPath, "ProcessedImages");
            Directory.CreateDirectory(defaultPath); // Створюємо директорію, якщо вона не існує

            MessageBox.Show("Images will be saved at " + defaultPath);
            return defaultPath;
        }
    }

    private void SaveImageSafely(Bitmap image, string filename)
    {
        lock (lockObject)
        {
            try
            {
                var savePath = Path.Combine(outputDirectory, filename);
                image.Save(savePath, ImageFormat.Jpeg);

                this.Invoke((MethodInvoker)delegate {
                    progressBar.Value++;
                    lblStatus.Text = $"Processed: {progressBar.Value}/{progressBar.Maximum}";
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate {
                    MessageBox.Show($"Error saving image {filename}: {ex.Message}");
                });
            }
        }
    }

}