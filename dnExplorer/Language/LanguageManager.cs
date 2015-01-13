using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace dnExplorer.Language {
	public class LanguageManager : INotifyPropertyChanged {
		public IList<ILanguage> Languages { get; private set; }

		ILanguage active;

		public ILanguage ActiveLanguage {
			get { return active; }
			set {
				if (value != null && active != value) {
					active = value;
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs("ActiveLanguage"));
				}
			}
		}

		public LanguageManager() {
			Languages = new ObservableCollection<ILanguage> {
				new CILLanguage(),
				(active = new CSharpLanguage())
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}