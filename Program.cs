using System.Collections.Concurrent;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HomeworkThreadingAsync;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private readonly Random _random = new();
    private readonly object _randomLock = new();

    private readonly NumericUpDown _barCountInput = new();
    private readonly FlowLayoutPanel _dancingPanel = new();
    private readonly Button _createBarsButton = new();
    private readonly Button _startBarsButton = new();
    private readonly List<DancingProgressBar> _dancingBars = new();

    private readonly Button _raceStartButton = new();
    private readonly FlowLayoutPanel _racePanel = new();
    private readonly DataGridView _raceResultsGrid = new();
    private readonly List<DancingProgressBar> _horseBars = new();

    private readonly TextBox _fibLimitTextBox = new();
    private readonly Button _fibCalculateButton = new();
    private readonly TextBox _fibResultTextBox = new();

    private readonly TextBox _fileWordTextBox = new();
    private readonly TextBox _filePathTextBox = new();
    private readonly Button _fileBrowseButton = new();
    private readonly Button _fileSearchButton = new();
    private readonly Label _fileResultLabel = new();

    private readonly TextBox _dirWordTextBox = new();
    private readonly TextBox _dirPathTextBox = new();
    private readonly Button _dirBrowseButton = new();
    private readonly Button _dirSearchButton = new();
    private readonly TextBox _dirResultTextBox = new();

    public MainForm()
    {
        Text = "Домашнє завдання: багатопотоковість та асинхронність";
        MinimumSize = new Size(980, 720);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateDancingBarsTab());
        tabs.TabPages.Add(CreateHorseRaceTab());
        tabs.TabPages.Add(CreateFibonacciTab());
        tabs.TabPages.Add(CreateFileSearchTab());
        tabs.TabPages.Add(CreateDirectorySearchTab());

        Controls.Add(tabs);
        CreateDancingBars();
        CreateHorseBars();
    }

    private TabPage CreateDancingBarsTab()
    {
        var page = new TabPage("1. Танцюючі прогрес-бари");
        var root = CreateRootPanel();

        var controls = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(14, 12, 14, 8),
            WrapContents = false
        };

        _barCountInput.Minimum = 1;
        _barCountInput.Maximum = 30;
        _barCountInput.Value = 8;
        _barCountInput.Width = 70;

        _createBarsButton.Text = "Створити";
        _createBarsButton.Width = 120;
        _createBarsButton.Click += (_, _) => CreateDancingBars();

        _startBarsButton.Text = "Старт";
        _startBarsButton.Width = 120;
        _startBarsButton.Click += (_, _) => StartDancingBars();

        controls.Controls.Add(CreateLabel("Кількість:"));
        controls.Controls.Add(_barCountInput);
        controls.Controls.Add(_createBarsButton);
        controls.Controls.Add(_startBarsButton);

        _dancingPanel.Dock = DockStyle.Fill;
        _dancingPanel.AutoScroll = true;
        _dancingPanel.Padding = new Padding(14);
        _dancingPanel.FlowDirection = FlowDirection.TopDown;
        _dancingPanel.WrapContents = false;

        root.Controls.Add(_dancingPanel);
        root.Controls.Add(controls);
        page.Controls.Add(root);
        return page;
    }

    private TabPage CreateHorseRaceTab()
    {
        var page = new TabPage("2. Кінні перегони");
        var root = CreateRootPanel();

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(14, 12, 14, 8) };
        _raceStartButton.Text = "Старт перегонів";
        _raceStartButton.Width = 160;
        _raceStartButton.Height = 32;
        _raceStartButton.Click += (_, _) => StartHorseRace();
        topPanel.Controls.Add(_raceStartButton);

        _raceResultsGrid.Dock = DockStyle.Bottom;
        _raceResultsGrid.Height = 210;
        _raceResultsGrid.AllowUserToAddRows = false;
        _raceResultsGrid.AllowUserToDeleteRows = false;
        _raceResultsGrid.ReadOnly = true;
        _raceResultsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _raceResultsGrid.RowHeadersVisible = false;
        _raceResultsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _raceResultsGrid.Columns.Add("Place", "Місце");
        _raceResultsGrid.Columns.Add("Horse", "Кінь");
        _raceResultsGrid.Columns.Add("Time", "Час");

        _racePanel.Dock = DockStyle.Fill;
        _racePanel.AutoScroll = true;
        _racePanel.Padding = new Padding(14);
        _racePanel.FlowDirection = FlowDirection.TopDown;
        _racePanel.WrapContents = false;

        root.Controls.Add(_racePanel);
        root.Controls.Add(_raceResultsGrid);
        root.Controls.Add(topPanel);
        page.Controls.Add(root);
        return page;
    }

    private TabPage CreateFibonacciTab()
    {
        var page = new TabPage("3. Фібоначчі");
        var root = CreateRootPanel();

        var controls = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(14, 12, 14, 8),
            WrapContents = false
        };

        _fibLimitTextBox.Width = 180;
        _fibLimitTextBox.Text = "100000";

        _fibCalculateButton.Text = "Порахувати";
        _fibCalculateButton.Width = 135;
        _fibCalculateButton.Click += async (_, _) => await CalculateFibonacciAsync();

        controls.Controls.Add(CreateLabel("Межа:"));
        controls.Controls.Add(_fibLimitTextBox);
        controls.Controls.Add(_fibCalculateButton);

        _fibResultTextBox.Dock = DockStyle.Fill;
        _fibResultTextBox.Multiline = true;
        _fibResultTextBox.ScrollBars = ScrollBars.Vertical;
        _fibResultTextBox.ReadOnly = true;
        _fibResultTextBox.Font = new Font("Consolas", 10F);
        _fibResultTextBox.Margin = new Padding(14);

        root.Controls.Add(_fibResultTextBox);
        root.Controls.Add(controls);
        page.Controls.Add(root);
        return page;
    }

    private TabPage CreateFileSearchTab()
    {
        var page = new TabPage("4. Пошук у файлі");
        var root = CreateRootPanel();
        var panel = CreateFormPanel();

        _fileWordTextBox.Width = 260;
        _filePathTextBox.Width = 560;

        _fileBrowseButton.Text = "Обрати файл";
        _fileBrowseButton.Width = 130;
        _fileBrowseButton.Click += (_, _) => BrowseFile();

        _fileSearchButton.Text = "Шукати";
        _fileSearchButton.Width = 130;
        _fileSearchButton.Click += async (_, _) => await SearchInFileAsync();

        _fileResultLabel.AutoSize = true;
        _fileResultLabel.Font = new Font(Font, FontStyle.Bold);
        _fileResultLabel.Padding = new Padding(0, 18, 0, 0);

        panel.Controls.Add(CreateRow("Слово:", _fileWordTextBox));
        panel.Controls.Add(CreatePathRow("Файл:", _filePathTextBox, _fileBrowseButton));
        panel.Controls.Add(CreateButtonRow(_fileSearchButton));
        panel.Controls.Add(_fileResultLabel);

        root.Controls.Add(panel);
        page.Controls.Add(root);
        return page;
    }

    private TabPage CreateDirectorySearchTab()
    {
        var page = new TabPage("5. Пошук у директорії");
        var root = CreateRootPanel();
        var top = CreateFormPanel();
        top.Dock = DockStyle.Top;
        top.Height = 170;

        _dirWordTextBox.Width = 260;
        _dirPathTextBox.Width = 560;

        _dirBrowseButton.Text = "Обрати папку";
        _dirBrowseButton.Width = 130;
        _dirBrowseButton.Click += (_, _) => BrowseDirectory();

        _dirSearchButton.Text = "Шукати";
        _dirSearchButton.Width = 130;
        _dirSearchButton.Click += async (_, _) => await SearchInDirectoryAsync();

        top.Controls.Add(CreateRow("Слово:", _dirWordTextBox));
        top.Controls.Add(CreatePathRow("Директорія:", _dirPathTextBox, _dirBrowseButton));
        top.Controls.Add(CreateButtonRow(_dirSearchButton));

        _dirResultTextBox.Dock = DockStyle.Fill;
        _dirResultTextBox.Multiline = true;
        _dirResultTextBox.ScrollBars = ScrollBars.Both;
        _dirResultTextBox.ReadOnly = true;
        _dirResultTextBox.Font = new Font("Consolas", 10F);
        _dirResultTextBox.WordWrap = false;

        root.Controls.Add(_dirResultTextBox);
        root.Controls.Add(top);
        page.Controls.Add(root);
        return page;
    }

    private void CreateDancingBars()
    {
        _dancingPanel.Controls.Clear();
        _dancingBars.Clear();

        for (var i = 1; i <= _barCountInput.Value; i++)
        {
            var bar = CreateNamedBar($"Прогрес-бар {i}", NextColor());
            _dancingBars.Add(bar);
            _dancingPanel.Controls.Add(bar);
        }
    }

    private void StartDancingBars()
    {
        if (_dancingBars.Count == 0)
        {
            CreateDancingBars();
        }

        _startBarsButton.Enabled = false;
        _createBarsButton.Enabled = false;

        var remaining = _dancingBars.Count;
        foreach (var bar in _dancingBars)
        {
            bar.Value = 0;
            bar.BarColor = NextColor();

            var thread = new Thread(() =>
            {
                var progress = 0;

                while (progress < 100)
                {
                    Thread.Sleep(NextInt(45, 180));
                    progress = Math.Min(100, progress + NextInt(1, 9));
                    var currentProgress = progress;
                    SafeUi(() => bar.Value = currentProgress);
                }

                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    SafeUi(() =>
                    {
                        _startBarsButton.Enabled = true;
                        _createBarsButton.Enabled = true;
                    });
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }
    }

    private void CreateHorseBars()
    {
        _racePanel.Controls.Clear();
        _horseBars.Clear();
        _raceResultsGrid.Rows.Clear();

        for (var i = 1; i <= 5; i++)
        {
            var bar = CreateNamedBar($"Кінь {i}", NextColor());
            _horseBars.Add(bar);
            _racePanel.Controls.Add(bar);
        }
    }

    private void StartHorseRace()
    {
        CreateHorseBars();
        _raceStartButton.Enabled = false;

        var results = new ConcurrentQueue<RaceResult>();
        var remaining = _horseBars.Count;
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < _horseBars.Count; i++)
        {
            var horseNumber = i + 1;
            var bar = _horseBars[i];

            var thread = new Thread(() =>
            {
                var progress = 0;

                while (progress < 100)
                {
                    Thread.Sleep(NextInt(60, 220));
                    progress = Math.Min(100, progress + NextInt(1, 8));
                    var currentProgress = progress;
                    SafeUi(() => bar.Value = currentProgress);
                }

                results.Enqueue(new RaceResult(horseNumber, stopwatch.Elapsed));

                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    stopwatch.Stop();
                    SafeUi(() =>
                    {
                        ShowRaceResults(results);
                        _raceStartButton.Enabled = true;
                    });
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }
    }

    private async Task CalculateFibonacciAsync()
    {
        if (!BigInteger.TryParse(_fibLimitTextBox.Text.Trim(), out var limit) || limit < 0)
        {
            MessageBox.Show("Введіть невід'ємне ціле число.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _fibCalculateButton.Enabled = false;
        _fibResultTextBox.Text = "Обчислення...";

        try
        {
            var numbers = await Task.Run(() => GetFibonacciNumbers(limit));
            _fibResultTextBox.Text = $"Кількість чисел: {numbers.Count}{Environment.NewLine}{Environment.NewLine}"
                + string.Join(Environment.NewLine, numbers);
        }
        finally
        {
            _fibCalculateButton.Enabled = true;
        }
    }

    private async Task SearchInFileAsync()
    {
        var word = _fileWordTextBox.Text.Trim();
        var path = _filePathTextBox.Text.Trim();

        if (!ValidateWordAndFile(word, path))
        {
            return;
        }

        _fileSearchButton.Enabled = false;
        _fileResultLabel.Text = "Пошук...";

        try
        {
            var count = await Task.Run(async () =>
            {
                var text = await File.ReadAllTextAsync(path);
                return CountWord(text, word);
            });

            _fileResultLabel.Text = $"Слово \"{word}\" зустрілося у файлі {count} раз(ів).";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Помилка читання файлу", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _fileResultLabel.Text = "";
        }
        finally
        {
            _fileSearchButton.Enabled = true;
        }
    }

    private async Task SearchInDirectoryAsync()
    {
        var word = _dirWordTextBox.Text.Trim();
        var path = _dirPathTextBox.Text.Trim();

        if (!ValidateWordAndDirectory(word, path))
        {
            return;
        }

        _dirSearchButton.Enabled = false;
        _dirResultTextBox.Text = "Пошук...";

        try
        {
            var results = await Task.Run(() => SearchDirectory(path, word));
            _dirResultTextBox.Text = FormatDirectoryReport(results);
        }
        finally
        {
            _dirSearchButton.Enabled = true;
        }
    }

    private void ShowRaceResults(IEnumerable<RaceResult> results)
    {
        _raceResultsGrid.Rows.Clear();

        var place = 1;
        foreach (var result in results.OrderBy(item => item.Time))
        {
            _raceResultsGrid.Rows.Add(place, $"Кінь {result.HorseNumber}", result.Time.ToString(@"mm\:ss\.fff"));
            place++;
        }
    }

    private List<FileSearchResult> SearchDirectory(string directory, string word)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true
        };

        var results = new List<FileSearchResult>();

        foreach (var file in Directory.EnumerateFiles(directory, "*.*", options))
        {
            try
            {
                var text = File.ReadAllText(file);
                var count = CountWord(text, word);

                if (count > 0)
                {
                    results.Add(new FileSearchResult(Path.GetFileName(file), file, count));
                }
            }
            catch
            {
                // Файл може бути зайнятий, бінарний або недоступний. Для звіту просто пропускаємо його.
            }
        }

        return results.OrderBy(item => item.Path).ToList();
    }

    private static string FormatDirectoryReport(IReadOnlyCollection<FileSearchResult> results)
    {
        if (results.Count == 0)
        {
            return "Збігів не знайдено.";
        }

        var builder = new StringBuilder();

        foreach (var result in results)
        {
            builder.AppendLine($"Назва файлу: {result.FileName}");
            builder.AppendLine($"Шлях до файлу: {result.Path}");
            builder.AppendLine($"Кількість входження слова: {result.Count}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static List<BigInteger> GetFibonacciNumbers(BigInteger limit)
    {
        var result = new List<BigInteger>();
        BigInteger first = 0;
        BigInteger second = 1;

        while (first <= limit)
        {
            result.Add(first);
            (first, second) = (second, first + second);
        }

        return result;
    }

    private static int CountWord(string text, string word)
    {
        var pattern = $@"(?<![\p{{L}}\p{{N}}_]){Regex.Escape(word)}(?![\p{{L}}\p{{N}}_])";
        return Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count;
    }

    private bool ValidateWordAndFile(string word, string path)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            MessageBox.Show("Введіть слово для пошуку.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!File.Exists(path))
        {
            MessageBox.Show("Файл не знайдено.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private bool ValidateWordAndDirectory(string word, string path)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            MessageBox.Show("Введіть слово для пошуку.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!Directory.Exists(path))
        {
            MessageBox.Show("Директорію не знайдено.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private void BrowseFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Оберіть файл для пошуку",
            Filter = "Текстові файли (*.txt;*.cs;*.md;*.json;*.xml)|*.txt;*.cs;*.md;*.json;*.xml|Усі файли (*.*)|*.*"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _filePathTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseDirectory()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Оберіть директорію для рекурсивного пошуку"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _dirPathTextBox.Text = dialog.SelectedPath;
        }
    }

    private DancingProgressBar CreateNamedBar(string text, Color color)
    {
        return new DancingProgressBar
        {
            Width = 880,
            Height = 38,
            Margin = new Padding(4, 6, 4, 6),
            Title = text,
            BarColor = color
        };
    }

    private static Panel CreateRootPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(247, 248, 250)
        };
    }

    private static FlowLayoutPanel CreateFormPanel()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(18),
            AutoScroll = true
        };
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 6, 8, 0)
        };
    }

    private static FlowLayoutPanel CreateRow(string labelText, Control input)
    {
        var row = new FlowLayoutPanel
        {
            Width = 880,
            Height = 42,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        var label = CreateLabel(labelText);
        label.Width = 100;
        input.Height = 28;

        row.Controls.Add(label);
        row.Controls.Add(input);
        return row;
    }

    private static FlowLayoutPanel CreatePathRow(string labelText, Control input, Control button)
    {
        var row = CreateRow(labelText, input);
        button.Height = 30;
        button.Margin = new Padding(8, 0, 0, 0);
        row.Controls.Add(button);
        return row;
    }

    private static FlowLayoutPanel CreateButtonRow(Control button)
    {
        var row = new FlowLayoutPanel
        {
            Width = 880,
            Height = 42,
            FlowDirection = FlowDirection.LeftToRight
        };

        button.Height = 32;
        button.Margin = new Padding(104, 4, 0, 0);
        row.Controls.Add(button);
        return row;
    }

    private Color NextColor()
    {
        return Color.FromArgb(255, NextInt(45, 225), NextInt(45, 225), NextInt(45, 225));
    }

    private int NextInt(int minValue, int maxValue)
    {
        lock (_randomLock)
        {
            return _random.Next(minValue, maxValue);
        }
    }

    private void SafeUi(Action action)
    {
        if (IsDisposed)
        {
            return;
        }

        try
        {
            BeginInvoke(action);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private sealed record RaceResult(int HorseNumber, TimeSpan Time);

    private sealed record FileSearchResult(string FileName, string Path, int Count);
}

public sealed class DancingProgressBar : Control
{
    private int _value;
    private Color _barColor = Color.SeaGreen;

    public DancingProgressBar()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        ForeColor = Color.FromArgb(34, 40, 49);
        Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    public string Title { get; set; } = "";

    public int Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, 0, 100);
            Invalidate();
        }
    }

    public Color BarColor
    {
        get => _barColor;
        set
        {
            _barColor = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;

        using var borderPen = new Pen(Color.FromArgb(210, 214, 220));
        using var backgroundBrush = new SolidBrush(Color.White);
        using var fillBrush = new SolidBrush(BarColor);
        using var textBrush = new SolidBrush(ForeColor);

        e.Graphics.FillRectangle(backgroundBrush, bounds);

        var fillWidth = (int)Math.Round(bounds.Width * Value / 100.0);
        if (fillWidth > 0)
        {
            e.Graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, fillWidth, bounds.Height);
        }

        e.Graphics.DrawRectangle(borderPen, bounds);

        var text = $"{Title}: {Value}%";
        var textSize = e.Graphics.MeasureString(text, Font);
        var textX = bounds.X + 12;
        var textY = bounds.Y + (bounds.Height - textSize.Height) / 2;

        using var shadowBrush = new SolidBrush(Color.FromArgb(230, Color.White));
        e.Graphics.DrawString(text, Font, shadowBrush, textX + 1, textY + 1);
        e.Graphics.DrawString(text, Font, textBrush, textX, textY);
    }
}
