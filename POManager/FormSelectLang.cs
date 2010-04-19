using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Streamlet.POManager.LanguageUtility;

namespace Streamlet.POManager.UI
{
    public partial class FormSelectLang : Form
    {
        public static CultureInfo Show()
        {
            FormSelectLang form = new FormSelectLang();
            return form.ShowDialog() == DialogResult.OK ? form._selectedLanguage : null;
        }

        private CultureInfo _selectedLanguage;

        private FormSelectLang()
        {
            InitializeComponent();
        }

        private void FormSelectLang_Load(object sender, EventArgs e)
        {
            AddToTree(treeLanguages.Nodes, LangUtility.TopLCID);
        }

        string s;

        private void AddToTree(TreeNodeCollection tnc, int parentLanguage, bool recursive = true)
        {
            foreach (var language in LangUtility.EnumSubLanguages(parentLanguage))
            {
                TreeNode tn = new TreeNode(language.DisplayName);
                tn.Tag = language.LCID;

                if (recursive)
                {
                    AddToTree(tn.Nodes, language.LCID, recursive);
                }

                tnc.Add(tn);
            }
            
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _selectedLanguage = new CultureInfo((int)treeLanguages.SelectedNode.Tag);
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
