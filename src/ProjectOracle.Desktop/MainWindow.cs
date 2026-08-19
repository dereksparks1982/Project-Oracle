using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ProjectOracle;
using ProjectOracle.Cognition.CosmicChoice;
using ProjectOracle.Cognition.Emergence;
using ProjectOracle.Domain;

namespace ProjectOracle.Desktop;

public sealed class MainWindow : Window
{
    private readonly OracleDesktopSession _session = new();
    private readonly TextBlock _clockText = MakeText("", 15, FontWeight.SemiBold);
    private readonly TextBlock _worldText = MakeText("", 14);
    private readonly TextBlock _mindText = MakeText("", 14);
    private readonly TextBlock _memoryText = MakeText("", 13);
    private readonly TextBlock _cosmologyText = MakeText("", 13);
    private readonly TextBlock _historyText = MakeText("", 13);
    private readonly TextBlock _lawsText = MakeText("", 13);
    private readonly TextBlock _debugText = MakeText("", 13);
    private readonly TextBlock _mindsText = MakeText("", 13);
    private readonly TextBlock _transcript = MakeText("", 15);
    private readonly TextBox _messageBox = new() { PlaceholderText = "Speak to Yala...", FontSize = 15, MinHeight = 42 };
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private int _seconds;

    public MainWindow()
    {
        Title = "Project Oracle";
        Width = 1440;
        Height = 900;
        MinWidth = 1050;
        MinHeight = 700;
        Background = new SolidColorBrush(Color.Parse("#090C0A"));
        Content = BuildLayout();
        _messageBox.KeyDown += OnMessageKeyDown;
        _timer.Tick += OnTimerTick;
        _timer.Start();
        Closing += (_, _) => _session.Save();
        Closed += (_, _) => _session.Dispose();
        LoadExistingDialogue();
        RefreshAll();
    }

    private Control BuildLayout()
    {
        Grid root = new()
        {
            RowDefinitions = new RowDefinitions("Auto,*,280"),
            Margin = new Thickness(14)
        };

        Grid header = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"), Margin = new Thickness(0, 0, 0, 10) };
        header.Children.Add(MakeHeader());
        Grid.SetColumn(_clockText, 1);
        _clockText.Margin = new Thickness(18, 8, 18, 0);
        header.Children.Add(_clockText);
        Button saveButton = MakeButton("SAVE", (_, _) => _session.Save());
        Grid.SetColumn(saveButton, 2);
        header.Children.Add(saveButton);
        Button freshButton = MakeButton("NEW FRESH WORLD", OnFreshWorld);
        Grid.SetColumn(freshButton, 3);
        freshButton.Margin = new Thickness(8, 0, 0, 0);
        header.Children.Add(freshButton);
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        Grid main = new() { ColumnDefinitions = new ColumnDefinitions("270,*,330"), ColumnSpacing = 10 };
        main.Children.Add(Panel("WORLD", Scroll(_worldText)));

        Control conversation = BuildConversationPanel();
        Grid.SetColumn(conversation, 1);
        main.Children.Add(conversation);

        Control mind = Panel("YALA MIND", Scroll(_mindText));
        Grid.SetColumn(mind, 2);
        main.Children.Add(mind);
        Grid.SetRow(main, 1);
        root.Children.Add(main);

        TabControl tabs = new()
        {
            Margin = new Thickness(0, 10, 0, 0),
            ItemsSource = new object[]
            {
                Tab("MINDS", _mindsText),
                Tab("MEMORY", _memoryText),
                Tab("COSMOLOGY", _cosmologyText),
                Tab("LAWS", _lawsText),
                Tab("HISTORY", _historyText),
                Tab("DEBUG", _debugText)
            }
        };
        Grid.SetRow(tabs, 2);
        root.Children.Add(tabs);
        return root;
    }

    private Control BuildConversationPanel()
    {
        Grid content = new() { RowDefinitions = new RowDefinitions("*,Auto"), RowSpacing = 8 };
        content.Children.Add(Scroll(_transcript));
        Grid input = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8 };
        input.Children.Add(_messageBox);
        Button speak = MakeButton("SPEAK", (_, _) => SendMessage());
        speak.MinWidth = 95;
        Grid.SetColumn(speak, 1);
        input.Children.Add(speak);
        Grid.SetRow(input, 1);
        content.Children.Add(input);
        return Panel("YALA / WORLD CONVERSATION", content);
    }

    private static TextBlock MakeHeader() => new()
    {
        Text = $"PROJECT ORACLE   v{ProjectVersion.Number}",
        FontSize = 23,
        FontWeight = FontWeight.Bold,
        Foreground = new SolidColorBrush(Color.Parse("#8AFF9B")),
        VerticalAlignment = VerticalAlignment.Center
    };

    private static TextBlock MakeText(string text, double size, FontWeight? weight = null) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight ?? FontWeight.Normal,
        Foreground = new SolidColorBrush(Color.Parse("#C9FFD1")),
        TextWrapping = TextWrapping.Wrap
    };

    private static Button MakeButton(string text, EventHandler<Avalonia.Interactivity.RoutedEventArgs> handler)
    {
        Button button = new()
        {
            Content = text,
            Padding = new Thickness(14, 8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#4C8F57")),
            Foreground = new SolidColorBrush(Color.Parse("#D7FFDC"))
        };
        button.Click += handler;
        return button;
    }

    private static Border Panel(string title, Control body)
    {
        Grid grid = new() { RowDefinitions = new RowDefinitions("Auto,*") };
        TextBlock heading = MakeText(title, 13, FontWeight.Bold);
        heading.Foreground = new SolidColorBrush(Color.Parse("#76E586"));
        heading.Margin = new Thickness(0, 0, 0, 8);
        grid.Children.Add(heading);
        Grid.SetRow(body, 1);
        grid.Children.Add(body);
        return new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#315A37")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(12),
            Background = new SolidColorBrush(Color.Parse("#101511")),
            Child = grid
        };
    }

    private static ScrollViewer Scroll(Control control) => new()
    {
        Content = control,
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
    };

    private static TabItem Tab(string header, TextBlock content) => new()
    {
        Header = header,
        Content = new Border { Padding = new Thickness(12), Child = Scroll(content) }
    };

    private void OnMessageKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            SendMessage();
        }
    }

    private void SendMessage()
    {
        string message = _messageBox.Text?.Trim() ?? string.Empty;
        if (message.Length == 0) return;
        AppendDialogue($"You: {message}");
        _messageBox.Text = string.Empty;
        try
        {
            var reply = _session.Speak(message);
            AppendDialogue($"Yala: {reply.Reply}");
        }
        catch (Exception error)
        {
            AppendDialogue($"SYSTEM: {error.GetBaseException().Message}");
        }
        RefreshAll();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        try
        {
            _session.Tick();
            _seconds++;
            if (_seconds % 5 == 0) _session.AutonomousStep();
            if (_session.Simulation.TryTakePendingYalaUtterance(out string? utterance) && !string.IsNullOrWhiteSpace(utterance))
            {
                AppendDialogue($"Yala: {utterance}");
            }
            RefreshAll();
        }
        catch (Exception error)
        {
            _timer.Stop();
            AppendDialogue($"SYSTEM: Automatic simulation stopped safely: {error.GetBaseException().Message}");
        }
    }

    private async void OnFreshWorld(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window confirm = new()
        {
            Title = "Start a fresh experimental Yala?",
            Width = 520,
            Height = 190,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        Grid grid = new() { RowDefinitions = new RowDefinitions("*,Auto"), Margin = new Thickness(18) };
        grid.Children.Add(MakeText("The current v0.0.23 save will be archived first. The new world starts with a fresh experimental Yala.", 14));
        StackPanel buttons = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
        Button cancel = MakeButton("CANCEL", (_, _) => confirm.Close(false));
        Button start = MakeButton("START FRESH", (_, _) => confirm.Close(true));
        buttons.Children.Add(cancel);
        buttons.Children.Add(start);
        Grid.SetRow(buttons, 1);
        grid.Children.Add(buttons);
        confirm.Content = grid;
        bool result = await confirm.ShowDialog<bool>(this);
        if (!result) return;
        _session.StartFreshWorld();
        _transcript.Text = string.Empty;
        AppendDialogue("SYSTEM: Fresh v0.0.23 experimental Yala started. Previous save archived.");
        RefreshAll();
    }

    private void LoadExistingDialogue()
    {
        foreach (YalaDialogueTurnState turn in _session.Simulation.State.YalaCognition?.Dialogue ?? [])
        {
            AppendDialogue($"You: {turn.Message}");
            if (!string.IsNullOrWhiteSpace(turn.Response)) AppendDialogue($"Yala: {turn.Response}");
        }
    }

    private void AppendDialogue(string line)
    {
        _transcript.Text = string.IsNullOrEmpty(_transcript.Text) ? line : $"{_transcript.Text}{Environment.NewLine}{Environment.NewLine}{line}";
    }

    private void RefreshAll()
    {
        var simulation = _session.Simulation;
        YalaCognitionState cognition = simulation.State.YalaCognition ?? ProjectOracle.Domain.WorldDefaults.CreateInitialYalaCognition();
        _clockText.Text = simulation.InWorldTimeExists
            ? $"In-world Time: {simulation.Clock.Calendar.DescribeDateAndTime()}"
            : "In-world Time: Gaia has not yet created Time.";

        _worldText.Text = string.Join(Environment.NewLine, simulation.State.CreationPowers
            .Where(item => item.Exists)
            .OrderBy(item => item.Order)
            .Select(item => $"{item.Name}{Environment.NewLine}  {item.Domain}"));

        YalaConcernState? concern = (cognition.Concerns ?? []).OrderByDescending(item => item.Priority).FirstOrDefault();
        YalaAppraisalState? appraisal = (cognition.Appraisals ?? []).OrderByDescending(item => item.Sequence).FirstOrDefault();
        YalaEntityModelState? speaker = (cognition.EntityModels ?? []).LastOrDefault(item => item.EntityKey == "unseen-speaker");
        _mindText.Text =
            $"ACTIVE CONCERN{Environment.NewLine}{(concern is null ? "None yet" : concern.Summary)}{Environment.NewLine}{Environment.NewLine}" +
            $"GOALS{Environment.NewLine}{Lines((cognition.Goals ?? []).Where(g => g.Status == "active").OrderByDescending(g => g.Priority).Take(6).Select(g => "• " + g.Goal))}{Environment.NewLine}{Environment.NewLine}" +
            $"APPRAISAL{Environment.NewLine}{(appraisal is null ? "No contact appraisal yet" : $"{appraisal.Primary} / {appraisal.Secondary}\n{appraisal.Summary}")}{Environment.NewLine}{Environment.NewLine}" +
            $"UNSEEN SPEAKER{Environment.NewLine}{(speaker is null ? "Identity and intent unresolved" : $"Trust: {speaker.TrustStatus}\nIntent: {speaker.IntentStatus}\nCapability: {speaker.CapabilityStatus}")}{Environment.NewLine}{Environment.NewLine}" +
            $"OPEN QUESTIONS{Environment.NewLine}{Lines((cognition.Questions ?? []).Where(q => !q.Asked).OrderByDescending(q => q.Priority).Take(6).Select(q => $"• [{q.Priority}] {q.Text}"))}";

        _mindsText.Text = "COGNITIVE LINEAGE\n\nYala: active descendant mind\nGaia and later created minds: not yet instantiated as independent minds.\n\nCreation inheritance architecture is present, with a strict rule that a created being begins below its creator's world-authority ceiling.\n\nFuture roadmap: Monad Primordial Mind research; Sophia root-descendant-mind research; neither future architecture is silently asserted as current world fact.";

        _memoryText.Text =
            $"INHERITED / SETTLED{Environment.NewLine}{Lines((cognition.Beliefs ?? []).Where(b => b.Status == "known").Take(12).Select(b => "• " + b.Proposition))}{Environment.NewLine}{Environment.NewLine}" +
            $"RECENT EPISODES{Environment.NewLine}{Lines((cognition.Episodes ?? []).TakeLast(10).Select(e => $"• {e.Summary}"))}{Environment.NewLine}{Environment.NewLine}" +
            $"REFLECTIONS{Environment.NewLine}{Lines((cognition.Reflections ?? []).TakeLast(8).Select(r => $"• {r.Summary}"))}";

        _cosmologyText.Text =
            $"ATTRIBUTED TRADITIONS: {YalaReligiousKnowledgeCatalog.Traditions.Count}{Environment.NewLine}" +
            $"ATTRIBUTED IDEAS: {YalaReligiousKnowledgeCatalog.Ideas.Count}{Environment.NewLine}" +
            $"COSMIC POSSIBILITIES: {YalaCosmicChoiceCatalog.Choices.Count}{Environment.NewLine}{Environment.NewLine}" +
            $"ESTABLISHED BY YALA{Environment.NewLine}{Lines((simulation.State.Cosmic?.EstablishedChoices ?? []).Select(c => $"• {c.Action}"))}{Environment.NewLine}{Environment.NewLine}" +
            "Religious material is attributed knowledge, not automatic world truth. Yala may combine, reject, or invent beyond the supplied possibilities.";

        OracleLawExperimentResult rule30 = Rule30Laboratory.RunSingleSeed(31, 8);
        IReadOnlyList<OracleEstablishedLawState> establishedLaws = simulation.State.EmergentLaws?.EstablishedLaws ?? [];
        _lawsText.Text =
            $"ORACLE EMERGENT LAW ENGINE{Environment.NewLine}{Environment.NewLine}" +
            $"ESTABLISHED WORLD LAWS{Environment.NewLine}{Lines(establishedLaws.Select(law => $"• {law.Name} [{law.Domain}] by {law.EstablishedBy}"))}{Environment.NewLine}{Environment.NewLine}" +
            $"RULE 30 LABORATORY DEMONSTRATION{Environment.NewLine}{string.Join(Environment.NewLine, rule30.Generations)}{Environment.NewLine}{Environment.NewLine}" +
            "Rule 30 is a demonstration only. It is not a law of this world. The engine separates available law mechanisms from laws actually established by authorised in-world creators.";

        _historyText.Text = Lines(simulation.Ledger.AllRecords.TakeLast(40).Select(r => $"#{r.Sequence} [{r.Category}] {r.Message}"));
        _debugText.Text =
            $"VERSION: {ProjectVersion.Display}{Environment.NewLine}" +
            $"SAVE: {_session.SavePath}{Environment.NewLine}" +
            $"SEED: {simulation.State.Seed}{Environment.NewLine}" +
            $"DECISIONS: {cognition.DecisionCount}{Environment.NewLine}" +
            $"LAST ACTION: {cognition.LastAction ?? "none"}{Environment.NewLine}" +
            $"LAST RESULT: {cognition.LastResult ?? "none"}{Environment.NewLine}" +
            $"CONCERNS: {(cognition.Concerns ?? []).Count}{Environment.NewLine}" +
            $"HYPOTHESES: {(cognition.Hypotheses ?? []).Count}{Environment.NewLine}" +
            $"QUESTIONS: {(cognition.Questions ?? []).Count}{Environment.NewLine}" +
            "\nDeveloper console remains available separately; normal Oracle use is this desktop application.";
    }

    private static string Lines(IEnumerable<string> lines)
    {
        string value = string.Join(Environment.NewLine, lines);
        return value.Length == 0 ? "None" : value;
    }
}
