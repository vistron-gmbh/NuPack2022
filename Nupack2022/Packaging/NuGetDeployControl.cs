using System;
using System.Linq;
using System.Windows.Forms;

namespace CnSharp.VisualStudio.NuPack.Packaging
{
    public partial class NuGetDeployControl : UserControl
    {
        private NuGetDeployViewModel _viewModel;
        private NuGetConfig _nuGetConfig;

        public NuGetDeployControl()
        {
            InitializeComponent();
            textBoxSymbolServer.Enabled = false;
            textBoxSymbolServer.TextChanged += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textBoxSymbolServer.Text))
                    textBoxSymbolServer.Enabled = true;
            };

            sourceBox.SelectedValueChanged += SourceBox_SelectedValueChanged;
        }

        private void SourceBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (sourceBox.SelectedItem is string url)
            {
                var match = _nuGetConfig.Sources.FirstOrDefault(x => x.Url == url);
                fileserverCheckBox.Checked = match.IsFileServer;
                _viewModel.TargetIsFileserver = match.IsFileServer;
            }
        }

        public NuGetConfig NuGetConfig
        {
            get { return _nuGetConfig; }
            set
            {
                _nuGetConfig = value;

                sourceBox.Items.Clear();
                foreach (var source in _nuGetConfig.Sources)                
                    sourceBox.Items.Add(source.Url);                

                //We set the combobox to the last index from the previous usage
                if (_nuGetConfig.Sources.FirstOrDefault(x => x.Url == _nuGetConfig.LastTarget) is NuGetSource lastTarget)
                    sourceBox.SelectedIndex = _nuGetConfig.Sources.IndexOf(lastTarget);               
            }
        }

        public NuGetDeployViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                    _viewModel = new NuGetDeployViewModel();
                _viewModel.NuGetServer = sourceBox.Text;
                _viewModel.RememberKey = chkRemember.Checked;
                _viewModel.ApiKey = textBoxApiKey.Text;
                _viewModel.V2Login = textBoxLogin.Text;
                _viewModel.TargetIsFileserver = fileserverCheckBox.Checked;
                return _viewModel;
            }
            set
            {
                _viewModel = value;

                sourceBox.DataBindings.Clear();
                sourceBox.DataBindings.Add("Text", _viewModel, "NuGetServer", true, DataSourceUpdateMode.OnPropertyChanged);
                textBoxApiKey.DataBindings.Clear();
                textBoxApiKey.DataBindings.Add("Text", _viewModel, "ApiKey", true, DataSourceUpdateMode.OnPropertyChanged);

                if (!string.IsNullOrWhiteSpace(_viewModel.SymbolServer))
                {
                    textBoxSymbolServer.Enabled = true;
                }
                textBoxSymbolServer.DataBindings.Clear();
                textBoxSymbolServer.DataBindings.Add("Text", _viewModel, "SymbolServer", true, DataSourceUpdateMode.OnPropertyChanged);

                textBoxLogin.DataBindings.Clear();
                textBoxLogin.DataBindings.Add("Text", _viewModel, "V2Login", true, DataSourceUpdateMode.OnPropertyChanged);

                fileserverCheckBox.DataBindings.Add("Checked", _viewModel, "TargetIsFileserver", true, DataSourceUpdateMode.OnPropertyChanged);
            }
        }

        private void sourceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var url = sourceBox.Text.Trim();
            var source = NuGetConfig.Sources.FirstOrDefault(m => m.Url == url);
            textBoxApiKey.Text = source?.ApiKey ?? string.Empty;
            chkRemember.Checked = textBoxApiKey.Text.Length > 0;
            textBoxLogin.Text = source?.UserName;
            checkBoxNugetLogin.Checked = textBoxLogin.Text.Length > 0;
        }


        private void checkBoxNugetLogin_CheckedChanged(object sender, EventArgs e)
        {
            var check = sender as CheckBox;

            textBoxLogin.Visible = check.Checked;
            labelLogin.Visible = check.Checked;
        }
    }

    public class NuGetDeployViewModel
    {
        public string NuGetServer { get; set; }
        public string ApiKey { get; set; }
        public bool RememberKey { get; set; }
        public string SymbolServer { get; set; }
        public string V2Login { get; set; }
        public bool TargetIsFileserver
        {
            get;
            set;
        }
    }
}
