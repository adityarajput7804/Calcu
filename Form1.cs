namespace WinFormsCalculator;

public partial class Form1 : Form
{
    private readonly TextBox _display;
    private decimal? _leftOperand;
    private string? _pendingOperator;
    private bool _startNewEntry = true;

    public Form1()
    {
        InitializeComponent();
        Text = "Calculator";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(320, 460);

        _display = new TextBox
        {
            ReadOnly = true,
            Text = "0",
            TextAlign = HorizontalAlignment.Right,
            Font = new Font("Segoe UI", 24F, FontStyle.Regular, GraphicsUnit.Point),
            Location = new Point(10, 10),
            Width = 300,
            Height = 56,
            TabStop = false
        };
        Controls.Add(_display);

        var layout = new TableLayoutPanel
        {
            Location = new Point(10, 80),
            Size = new Size(300, 360),
            ColumnCount = 4,
            RowCount = 5
        };

        for (var column = 0; column < 4; column++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        }

        for (var row = 0; row < 5; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        }

        AddButton(layout, "C", 0, 0, ClearAllClicked);
        AddButton(layout, "<-", 1, 0, BackspaceClicked);
        AddButton(layout, "+/-", 2, 0, ToggleSignClicked);
        AddButton(layout, "/", 3, 0, OperatorClicked);

        AddButton(layout, "7", 0, 1, DigitClicked);
        AddButton(layout, "8", 1, 1, DigitClicked);
        AddButton(layout, "9", 2, 1, DigitClicked);
        AddButton(layout, "*", 3, 1, OperatorClicked);

        AddButton(layout, "4", 0, 2, DigitClicked);
        AddButton(layout, "5", 1, 2, DigitClicked);
        AddButton(layout, "6", 2, 2, DigitClicked);
        AddButton(layout, "-", 3, 2, OperatorClicked);

        AddButton(layout, "1", 0, 3, DigitClicked);
        AddButton(layout, "2", 1, 3, DigitClicked);
        AddButton(layout, "3", 2, 3, DigitClicked);
        AddButton(layout, "+", 3, 3, OperatorClicked);

        AddButton(layout, "0", 0, 4, DigitClicked);
        layout.SetColumnSpan(layout.Controls[^1], 2);
        AddButton(layout, ".", 2, 4, DecimalClicked);
        AddButton(layout, "=", 3, 4, EqualsClicked);

        Controls.Add(layout);
    }

    private void AddButton(TableLayoutPanel panel, string text, int column, int row, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(4)
        };

        button.Click += onClick;
        panel.Controls.Add(button, column, row);
    }

    private void DigitClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (_startNewEntry || _display.Text == "0")
        {
            _display.Text = button.Text;
            _startNewEntry = false;
            return;
        }

        _display.Text += button.Text;
    }

    private void DecimalClicked(object? sender, EventArgs e)
    {
        if (_startNewEntry)
        {
            _display.Text = "0.";
            _startNewEntry = false;
            return;
        }

        if (!_display.Text.Contains('.'))
        {
            _display.Text += ".";
        }
    }

    private void OperatorClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (!decimal.TryParse(_display.Text, out var currentValue))
        {
            return;
        }

        if (_leftOperand.HasValue && !_startNewEntry)
        {
            var chainedResult = Calculate(_leftOperand.Value, currentValue, _pendingOperator);
            _display.Text = chainedResult;

            if (!decimal.TryParse(chainedResult, out var parsedResult))
            {
                ResetAfterError();
                return;
            }

            _leftOperand = parsedResult;
        }
        else
        {
            _leftOperand = currentValue;
        }

        _pendingOperator = button.Text;
        _startNewEntry = true;
    }

    private void EqualsClicked(object? sender, EventArgs e)
    {
        if (!_leftOperand.HasValue || string.IsNullOrWhiteSpace(_pendingOperator))
        {
            return;
        }

        if (!decimal.TryParse(_display.Text, out var rightOperand))
        {
            return;
        }

        var result = Calculate(_leftOperand.Value, rightOperand, _pendingOperator);
        _display.Text = result;
        _leftOperand = null;
        _pendingOperator = null;
        _startNewEntry = true;
    }

    private string Calculate(decimal left, decimal right, string? op)
    {
        return op switch
        {
            "+" => (left + right).ToString(),
            "-" => (left - right).ToString(),
            "*" => (left * right).ToString(),
            "/" when right == 0 => "Cannot divide by zero",
            "/" => (left / right).ToString(),
            _ => right.ToString()
        };
    }

    private void ClearAllClicked(object? sender, EventArgs e)
    {
        _display.Text = "0";
        _leftOperand = null;
        _pendingOperator = null;
        _startNewEntry = true;
    }

    private void BackspaceClicked(object? sender, EventArgs e)
    {
        if (_startNewEntry || string.IsNullOrEmpty(_display.Text) || _display.Text == "0")
        {
            return;
        }

        if (_display.Text.Length == 1 || (_display.Text.Length == 2 && _display.Text.StartsWith('-')))
        {
            _display.Text = "0";
            _startNewEntry = true;
            return;
        }

        _display.Text = _display.Text[..^1];
    }

    private void ToggleSignClicked(object? sender, EventArgs e)
    {
        if (!decimal.TryParse(_display.Text, out var value) || value == 0)
        {
            return;
        }

        _display.Text = (-value).ToString();
    }

    private void ResetAfterError()
    {
        _leftOperand = null;
        _pendingOperator = null;
        _startNewEntry = true;
    }
}
