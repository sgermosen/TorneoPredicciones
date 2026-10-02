namespace CompeTournament.Mobile
{
    public partial class App : Application
    {
        private readonly AppShell _shell;

        public App(AppShell shell)
        {
            InitializeComponent();
            _shell = shell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_shell);
        }
    }
}
