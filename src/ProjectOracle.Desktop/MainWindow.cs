using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ProjectOracle;
using ProjectOracle.Cognition.CosmicChoice;
using ProjectOracle.Cognition.Emergence;
using ProjectOracle.Cognition.Planning;
using ProjectOracle.Domain;

namespace ProjectOracle.Desktop;

public sealed class MainWindow : Window
{
    private readonly OracleDesktopSession _session = new();
    private readonly TextBlock _clockText = MakeText("", 14, FontWeight.SemiBold);
    private readonly TextBlock _worldText = MakeText("", 13);
    private readonly TextBlock _mindText = MakeText("", 13);
    private readonly TextBlock _memoryText = MakeText("", 13);
    private readonly TextBlock _cosmologyText = MakeText("", 13);
    private readonly TextBlock _historyText = MakeText("", 13);
    private readonly TextBlock _lawsText = MakeText("", 13);
    private readonly TextBlock _debugText = MakeText("", 13);
    private readonly TextBlock _mindsText = MakeText("", 13);
    private readonly SelectableTextBlock _transcript = new()
    {
        FontSize = 15,
        Foreground = new SolidColorBrush(Color.Parse("#C9FFD1")),
        TextWrapping = TextWrapping.Wrap
    };
    private readonly TextBlock _statusText = MakeText("Ready.", 11);
    private readonly TextBox _messageBox = new() { PlaceholderText = "Speak to Yala...", FontSize = 15, MinHeight = 40 };
    private readonly ScrollViewer _conversationScroll;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private Grid? _mainGrid;
    private Grid? _rootGrid;
    private bool _autoFollow = true;
    private bool _programmaticScroll;
    private double _lastConversationOffsetY;
    private int _seconds;

    public MainWindow()
    {
        Title = "Project Oracle";
        Width = 1180;
        Height = 650;
        MinWidth = 640;
        MinHeight = 420;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.Parse("#090C0A"));
        Icon = LoadWindowIcon("avares://ProjectOracle/Assets/project-oracle-icon.png");

        _conversationScroll = Scroll(_transcript);
        _conversationScroll.ScrollChanged += OnConversationScrollChanged;

        Content = BuildLayout();
        _messageBox.KeyDown += OnMessageKeyDown;
        _timer.Tick += OnTimerTick;
        _timer.Start();
        Opened += OnOpened;
        SizeChanged += (_, _) => ApplyResponsiveLayout();
        Closing += (_, _) =>
        {
            OracleWindowPlacementStore.Save(this);
            _session.Save();
        };
        Closed += (_, _) => _session.Dispose();

        LoadExistingDialogue();
        RefreshAll();
        Dispatcher.UIThread.Post(JumpToLatest, DispatcherPriority.Background);
    }

    private Control BuildLayout()
    {
        _rootGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,210"),
            Margin = new Thickness(12)
        };

        Control header = BuildHeader();
        Grid.SetRow(header, 0);
        _rootGrid.Children.Add(header);

        _mainGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("220,*,290"), ColumnSpacing = 8 };
        _mainGrid.Children.Add(Panel("WORLD", BuildWorldPanel()));

        Control conversation = BuildConversationPanel();
        Grid.SetColumn(conversation, 1);
        _mainGrid.Children.Add(conversation);

        Control mind = Panel("YALA MIND", Scroll(_mindText));
        Grid.SetColumn(mind, 2);
        _mainGrid.Children.Add(mind);
        Grid.SetRow(_mainGrid, 1);
        _rootGrid.Children.Add(_mainGrid);

        TabControl tabs = new()
        {
            Margin = new Thickness(0, 8, 0, 0),
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
        _rootGrid.Children.Add(tabs);
        return _rootGrid;
    }

    private Control BuildHeader()
    {
        Grid header = new()
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 0, 0, 8)
        };
        header.Children.Add(MakeHeader());
        Grid.SetColumn(_clockText, 1);
        _clockText.Margin = new Thickness(12, 6, 0, 0);
        header.Children.Add(_clockText);

        WrapPanel controls = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 6, 0, 0)
        };
        controls.Children.Add(MakeButton("SAVE", (_, _) =>
        {
            _session.Save();
            SetStatus("World saved.");
        }));
        controls.Children.Add(SpacedButton("EXPORT SESSION JSON", (_, _) => ExportJson()));
        controls.Children.Add(SpacedButton("EXPORT CONVERSATION", (_, _) => ExportText()));
        controls.Children.Add(SpacedButton("FIT TO SCREEN", (_, _) => FitToScreen(center: true)));
        controls.Children.Add(SpacedButton("NEW FRESH WORLD", OnFreshWorld));
        Grid.SetRow(controls, 1);
        Grid.SetColumnSpan(controls, 2);
        header.Children.Add(controls);
        return header;
    }

    private Control BuildWorldPanel()
    {
        Grid body = new() { RowDefinitions = new RowDefinitions("Auto,*"), RowSpacing = 8 };
        Image icon = new()
        {
            Source = LoadBitmap("avares://ProjectOracle/Assets/project-oracle-eye-128.png"),
            Width = 72,
            Height = 72,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        body.Children.Add(icon);
        ScrollViewer worldScroll = Scroll(_worldText);
        Grid.SetRow(worldScroll, 1);
        body.Children.Add(worldScroll);
        return body;
    }

    private Control BuildConversationPanel()
    {
        Grid outer = new() { RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"), RowSpacing = 7 };
        Grid conversationHeader = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        TextBlock heading = MakeText("YALA / WORLD CONVERSATION", 13, FontWeight.Bold);
        heading.Foreground = new SolidColorBrush(Color.Parse("#76E586"));
        conversationHeader.Children.Add(heading);
        Button latest = MakeButton("JUMP TO LATEST", (_, _) => JumpToLatest());
        latest.Padding = new Thickness(10, 5);
        Grid.SetColumn(latest, 1);
        conversationHeader.Children.Add(latest);
        outer.Children.Add(conversationHeader);

        Grid.SetRow(_conversationScroll, 1);
        outer.Children.Add(_conversationScroll);

        Grid input = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8 };
        input.Children.Add(_messageBox);
        Button speak = MakeButton("SPEAK", (_, _) => SendMessage());
        speak.MinWidth = 88;
        Grid.SetColumn(speak, 1);
        input.Children.Add(speak);
        Grid.SetRow(input, 2);
        outer.Children.Add(input);

        _statusText.Foreground = new SolidColorBrush(Color.Parse("#87A98C"));
        Grid.SetRow(_statusText, 3);
        outer.Children.Add(_statusText);

        return new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#315A37")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Color.Parse("#101511")),
            Child = outer
        };
    }

    private static TextBlock MakeHeader() => new()
    {
        Text = $"PROJECT ORACLE   v{ProjectVersion.Number}",
        FontSize = 22,
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
            Padding = new Thickness(12, 7),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#4C8F57")),
            Foreground = new SolidColorBrush(Color.Parse("#D7FFDC"))
        };
        button.Click += handler;
        return button;
    }

    private static Button SpacedButton(string text, EventHandler<Avalonia.Interactivity.RoutedEventArgs> handler)
    {
        Button button = MakeButton(text, handler);
        button.Margin = new Thickness(7, 0, 0, 0);
        return button;
    }

    private static Border Panel(string title, Control body)
    {
        Grid grid = new() { RowDefinitions = new RowDefinitions("Auto,*") };
        TextBlock heading = MakeText(title, 13, FontWeight.Bold);
        heading.Foreground = new SolidColorBrush(Color.Parse("#76E586"));
        heading.Margin = new Thickness(0, 0, 0, 7);
        grid.Children.Add(heading);
        Grid.SetRow(body, 1);
        grid.Children.Add(body);
        return new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#315A37")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10),
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
        Content = new Border { Padding = new Thickness(10), Child = Scroll(content) }
    };

    private void OnOpened(object? sender, EventArgs e)
    {
        if (!OracleWindowPlacementStore.TryRestore(this)) FitToScreen(center: true);
        else ClampWindowToScreen();
        ApplyResponsiveLayout();
    }

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
        _autoFollow = true;
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
            Width = 500,
            Height = 185,
            MinWidth = 360,
            MinHeight = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Icon = LoadWindowIcon("avares://ProjectOracle/Assets/project-oracle-icon-64.png")
        };
        Grid grid = new() { RowDefinitions = new RowDefinitions("*,Auto"), Margin = new Thickness(18) };
        grid.Children.Add(MakeText("The current v0.0.24 save will be archived first. The new world starts with a fresh experimental Yala.", 14));
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
        _autoFollow = true;
        AppendDialogue("SYSTEM: Fresh v0.0.24 experimental Yala started. Previous save archived.");
        RefreshAll();
    }

    private void ExportJson()
    {
        try
        {
            string path = _session.ExportSessionJson();
            SetStatus($"Session JSON exported: {path}");
        }
        catch (Exception error)
        {
            SetStatus($"Export failed: {error.GetBaseException().Message}");
        }
    }

    private void ExportText()
    {
        try
        {
            string path = _session.ExportConversationText();
            SetStatus($"Conversation exported: {path}");
        }
        catch (Exception error)
        {
            SetStatus($"Export failed: {error.GetBaseException().Message}");
        }
    }

    private void LoadExistingDialogue()
    {
        foreach (YalaDialogueTurnState turn in _session.Simulation.State.YalaCognition?.Dialogue ?? [])
        {
            AppendDialogue($"You: {turn.Message}", scroll: false);
            if (!string.IsNullOrWhiteSpace(turn.Response)) AppendDialogue($"Yala: {turn.Response}", scroll: false);
        }
    }

    private void AppendDialogue(string line, bool scroll = true)
    {
        _transcript.Text = string.IsNullOrEmpty(_transcript.Text)
            ? line
            : $"{_transcript.Text}{Environment.NewLine}{Environment.NewLine}{line}";
        if (scroll && _autoFollow)
        {
            Dispatcher.UIThread.Post(ScrollToLatest, DispatcherPriority.Background);
        }
    }

    private void OnConversationScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        double current = _conversationScroll.Offset.Y;
        double bottom = Math.Max(0, _conversationScroll.Extent.Height - _conversationScroll.Viewport.Height);
        if (!_programmaticScroll && current + 1 < _lastConversationOffsetY) _autoFollow = false;
        if (current >= bottom - 8) _autoFollow = true;
        _lastConversationOffsetY = current;
    }

    private void JumpToLatest()
    {
        _autoFollow = true;
        ScrollToLatest();
    }

    private void ScrollToLatest()
    {
        _programmaticScroll = true;
        try
        {
            _conversationScroll.ScrollToEnd();
            _lastConversationOffsetY = _conversationScroll.Offset.Y;
        }
        finally
        {
            _programmaticScroll = false;
        }
    }

    private void FitToScreen(bool center)
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) return;
        double scaling = RenderScaling <= 0 ? 1.0 : RenderScaling;
        PixelRect work = screen.WorkingArea;
        double availableWidth = Math.Max(320, work.Width / scaling);
        double availableHeight = Math.Max(260, work.Height / scaling);
        MinWidth = Math.Min(availableWidth, Math.Min(760, Math.Max(420, availableWidth * 0.55)));
        MinHeight = Math.Min(availableHeight, Math.Min(500, Math.Max(320, availableHeight * 0.55)));
        Width = Math.Max(MinWidth, Math.Min(1200, availableWidth * 0.94));
        Height = Math.Max(MinHeight, Math.Min(680, availableHeight * 0.92));
        if (center)
        {
            int pixelWidth = (int)Math.Round(Width * scaling);
            int pixelHeight = (int)Math.Round(Height * scaling);
            Position = new PixelPoint(
                work.X + Math.Max(0, (work.Width - pixelWidth) / 2),
                work.Y + Math.Max(0, (work.Height - pixelHeight) / 2));
        }
        ApplyResponsiveLayout();
    }

    private void ClampWindowToScreen()
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) return;
        double scaling = RenderScaling <= 0 ? 1.0 : RenderScaling;
        PixelRect work = screen.WorkingArea;
        double maxWidth = work.Width / scaling;
        double maxHeight = work.Height / scaling;
        if (Width > maxWidth || Height > maxHeight)
        {
            FitToScreen(center: true);
            return;
        }
        int pixelWidth = (int)Math.Round(Width * scaling);
        int pixelHeight = (int)Math.Round(Height * scaling);
        int x = Math.Clamp(Position.X, work.X, Math.Max(work.X, work.Right - pixelWidth));
        int y = Math.Clamp(Position.Y, work.Y, Math.Max(work.Y, work.Bottom - pixelHeight));
        Position = new PixelPoint(x, y);
    }

    private void ApplyResponsiveLayout()
    {
        if (_mainGrid is null || _rootGrid is null) return;
        double width = Bounds.Width > 0 ? Bounds.Width : Width;
        double height = Bounds.Height > 0 ? Bounds.Height : Height;
        double aspectRatio = height <= 0 ? 1.0 : width / height;
        if (width < 900 || aspectRatio < 1.45)
        {
            _mainGrid.ColumnDefinitions[0].Width = new GridLength(160);
            _mainGrid.ColumnDefinitions[2].Width = new GridLength(205);
        }
        else if (width < 1150 || aspectRatio < 1.65)
        {
            _mainGrid.ColumnDefinitions[0].Width = new GridLength(195);
            _mainGrid.ColumnDefinitions[2].Width = new GridLength(250);
        }
        else
        {
            _mainGrid.ColumnDefinitions[0].Width = new GridLength(220);
            _mainGrid.ColumnDefinitions[2].Width = new GridLength(290);
        }
        _rootGrid.RowDefinitions[2].Height = new GridLength(height < 620 ? 165 : 210);
    }

    private void RefreshAll()
    {
        var simulation = _session.Simulation;
        YalaCognitionState cognition = simulation.State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
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
        YalaPlanState? plan = YalaDeliberationPlanner.SelectActivePlan(cognition);
        YalaPlanStepState? planStep = plan?.Steps.FirstOrDefault(item => item.Order == plan.CurrentStepOrder);
        YalaInvestigationState? investigation = YalaDeliberationPlanner.SelectActiveInvestigation(cognition);
        YalaDecisionTraceState? trace = (cognition.DecisionTrace ?? []).LastOrDefault();

        _mindText.Text =
            $"CURRENT PROBLEM{Environment.NewLine}{(investigation?.Question ?? concern?.Summary ?? "None yet")}{Environment.NewLine}{Environment.NewLine}" +
            $"ACTIVE PLAN{Environment.NewLine}{(plan is null ? "None yet" : $"{plan.Goal}\nNext: {planStep?.Action}\nWhy: {planStep?.Rationale}")}{Environment.NewLine}{Environment.NewLine}" +
            $"LAST DECISION{Environment.NewLine}{(trace is null ? "No recorded decision yet" : $"{trace.SelectedAction}\n{trace.Rationale}")}{Environment.NewLine}{Environment.NewLine}" +
            $"APPRAISAL{Environment.NewLine}{(appraisal is null ? "No contact appraisal yet" : $"{appraisal.Primary} / {appraisal.Secondary}\n{appraisal.Summary}")}{Environment.NewLine}{Environment.NewLine}" +
            $"UNSEEN SPEAKER{Environment.NewLine}{(speaker is null ? "Identity and intent unresolved" : $"Trust: {speaker.TrustStatus}\nIntent: {speaker.IntentStatus}\nCapability: {speaker.CapabilityStatus}")}{Environment.NewLine}{Environment.NewLine}" +
            $"GOALS{Environment.NewLine}{Lines((cognition.Goals ?? []).Where(g => g.Status == "active").OrderByDescending(g => g.Priority).Take(5).Select(g => "• " + g.Goal))}{Environment.NewLine}{Environment.NewLine}" +
            $"OPEN QUESTIONS{Environment.NewLine}{Lines((cognition.Questions ?? []).Where(q => !q.Asked).OrderByDescending(q => q.Priority).Take(5).Select(q => $"• [{q.Priority}] {q.Text}"))}";

        _mindsText.Text = "COGNITIVE LINEAGE\n\nYala: active descendant mind\nGaia and later created minds: not yet instantiated as independent minds.\n\nCreation inheritance architecture is present, with a strict rule that a created being begins below its creator's world-authority ceiling.\n\nFuture roadmap: Monad Primordial Mind research; Sophia root-descendant-mind research; neither future architecture is silently asserted as current world fact.";

        _memoryText.Text =
            $"INHERITED / SETTLED{Environment.NewLine}{Lines((cognition.Beliefs ?? []).Where(b => b.Status == "known").Take(12).Select(b => "• " + b.Proposition))}{Environment.NewLine}{Environment.NewLine}" +
            $"RECENT EPISODES{Environment.NewLine}{Lines((cognition.Episodes ?? []).TakeLast(10).Select(e => $"• {e.Summary}"))}{Environment.NewLine}{Environment.NewLine}" +
            $"REFLECTIONS / DELIBERATION{Environment.NewLine}{Lines((cognition.Reflections ?? []).TakeLast(8).Select(r => $"• {r.Summary}"))}";

        _cosmologyText.Text =
            $"ATTRIBUTED TRADITIONS: {YalaReligiousKnowledgeCatalog.Traditions.Count}{Environment.NewLine}" +
            $"ATTRIBUTED IDEAS: {YalaReligiousKnowledgeCatalog.Ideas.Count}{Environment.NewLine}" +
            $"COSMIC POSSIBILITIES: {YalaCosmicChoiceCatalog.Choices.Count}{Environment.NewLine}{Environment.NewLine}" +
            $"ESTABLISHED BY YALA{Environment.NewLine}{Lines((simulation.State.Cosmic?.EstablishedChoices ?? []).Select(c => $"• {c.Action}"))}{Environment.NewLine}{Environment.NewLine}" +
            $"COUNTERFACTUALS UNDER CONSIDERATION{Environment.NewLine}{Lines((cognition.Counterfactuals ?? []).TakeLast(6).Select(c => $"• {c.Option}: benefit={c.PossibleBenefit} risk={c.PossibleRisk}"))}{Environment.NewLine}{Environment.NewLine}" +
            "Religious material is attributed knowledge, not automatic world truth. Yala may combine, reject, or invent beyond the supplied possibilities.";

        OracleLawExperimentResult rule30 = Rule30Laboratory.RunSingleSeed(31, 8);
        IReadOnlyList<OracleEstablishedLawState> establishedLaws = simulation.State.EmergentLaws?.EstablishedLaws ?? [];
        _lawsText.Text =
            $"ORACLE EMERGENT LAW ENGINE{Environment.NewLine}{Environment.NewLine}" +
            $"ESTABLISHED WORLD LAWS{Environment.NewLine}{Lines(establishedLaws.Select(law => $"• {law.Name} [{law.Domain}] by {law.EstablishedBy}"))}{Environment.NewLine}{Environment.NewLine}" +
            $"RULE 30 LABORATORY DEMONSTRATION{Environment.NewLine}{string.Join(Environment.NewLine, rule30.Generations)}{Environment.NewLine}{Environment.NewLine}" +
            "Rule 30 is a demonstration only. It is not a law of this world. The engine separates available law mechanisms from laws actually established by authorised in-world creators.";

        _historyText.Text =
            $"SESSION EXPORTS{Environment.NewLine}Use EXPORT SESSION JSON for the cognitive flight recorder or EXPORT CONVERSATION for a readable transcript.{Environment.NewLine}{Environment.NewLine}" +
            Lines(simulation.Ledger.AllRecords.TakeLast(40).Select(r => $"#{r.Sequence} [{r.Category}] {r.Message}"));

        _debugText.Text =
            $"VERSION: {ProjectVersion.Display}{Environment.NewLine}" +
            $"SAVE: {_session.SavePath}{Environment.NewLine}" +
            $"SEED: {simulation.State.Seed}{Environment.NewLine}" +
            $"DECISIONS: {cognition.DecisionCount}{Environment.NewLine}" +
            $"LAST ACTION: {cognition.LastAction ?? "none"}{Environment.NewLine}" +
            $"LAST RESULT: {cognition.LastResult ?? "none"}{Environment.NewLine}" +
            $"PLANS: {(cognition.Plans ?? []).Count}{Environment.NewLine}" +
            $"INVESTIGATIONS: {(cognition.Investigations ?? []).Count}{Environment.NewLine}" +
            $"COUNTERFACTUALS: {(cognition.Counterfactuals ?? []).Count}{Environment.NewLine}" +
            $"FLIGHT RECORDER ENTRIES: {(cognition.DecisionTrace ?? []).Count}{Environment.NewLine}{Environment.NewLine}" +
            $"RECENT DECISION TRACE{Environment.NewLine}{Lines((cognition.DecisionTrace ?? []).TakeLast(8).Select(t => $"• #{t.Sequence} {t.Trigger}: {t.SelectedAction} | {t.Rationale}"))}{Environment.NewLine}{Environment.NewLine}" +
            "Developer console remains available separately; normal Oracle use is this desktop application.";
    }

    private void SetStatus(string text)
    {
        _statusText.Text = text;
    }

    private static string Lines(IEnumerable<string> lines)
    {
        string value = string.Join(Environment.NewLine, lines);
        return value.Length == 0 ? "None" : value;
    }

    private static Bitmap LoadBitmap(string uri)
    {
        using Stream stream = AssetLoader.Open(new Uri(uri));
        return new Bitmap(stream);
    }

    private static WindowIcon LoadWindowIcon(string uri)
    {
        using Stream stream = AssetLoader.Open(new Uri(uri));
        return new WindowIcon(stream);
    }
}
