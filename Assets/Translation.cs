using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Localization;

using System.Text.RegularExpressions;


[ExecuteInEditMode]

public class Translation : MonoBehaviour
{
	[System.Serializable]
	public class Segment2
	{
		public string key;
		public string original;
		public string parced;
		public List<string> forms	=	new List<string>();
	}

	[System.Serializable]
	public class Segment
	{
		public string key;
		public string textOriginal;
		public string text;
		public List<string> forms	=	new List<string>();

		public Segment (string _textOriginal, string _key)
		{
			key					=	_key;

			textOriginal		=	_textOriginal;
			text				=	"";

			var	textEngN		=	textOriginal.Replace("\\n\\n\\n", "[n3]");
				textEngN		=	textEngN.Replace("\\n\\n", "[n2]");
				textEngN		=	textEngN.Replace("\\n", "[n]");

			
			var	skobkaOpen		=	0;
			var	skobkaInside	=	"";

			for (var i=0; i<textEngN.Length; i++)
			{
				if (textEngN[i] == '[')
				{
					skobkaOpen++;

					if (skobkaOpen == 1)
						text			+=	'[';
				}

				if (textEngN[i] == ']')
				{
					//	конец
					if (i == textEngN.Length-1)
					{
						//text			+=	']';
						skobkaInside	+=	']';

						forms.Add(skobkaInside);

						return;
					}

					//	закрывается и сразу открывается
					if (textEngN[i+1] == '[')
					{
						skobkaInside	+=	"][";
						i++;
						continue;
					}

					//	закрывается и сразу открывается c символом или пробелом
					if (i <= textEngN.Length-3 && textEngN[i+2] == '[')
					{
						skobkaInside	+=	"]" + textEngN[i+1] + "[";
						i++;
						i++;
						continue;
					}

					skobkaOpen--;

					if (skobkaOpen == 0)
					{
						skobkaInside	+=	']';

						forms.Add(skobkaInside);

						skobkaInside	=	"";

						continue;
					}
				}
					
				if (skobkaOpen > 0)
					skobkaInside	+=	textEngN[i];
				else
					text			+=	textEngN[i];
			}
		}
	}

	public	bool	parce;
	public	bool	pack;

	public	bool	show;

	public	string	folder			=	"Assets/Dialogues/";
	public	string	fileName		=	"Dialogues.csv";
	public	int		smallNum		=	3;
	public	int		rus				=	9;

	int		max_		=	18;

	//string	sPattern_	=	@"(<[^>]*>)+|\{.*?\}+|(\[.*?\])+|(\\n)+|(\@[a-zA-Z]+[0-9]+)";


	//var	sBrackets1	=	@"\{.*?\}";
			//var sPattern	=	@"<[^>]+>\s+(?=<)|<[^>]+>";	//	делит на каждый <>
			//var sPattern	=	@"<[^>]*>";

			//var sPattern	=	@"<[^>]*>|\{.*?\}|\\n";

	//string	sPattern_	=	@"<[^>]*>+|\{.*?\}+|(\[.*?\])+|(\\n)+|(\@[a-zA-Z]+[0-9]+)";
	string	sPattern_	=	@"(<[^>]*>+|\{.*?\}+|\[.*?\]+|\\n|\@[a-zA-Z]+[0-9]+)+";

	

    //
	void Update()
	{
		if (parce) {
			parce	=	false;
			Parce();

			Debug.Log("готово parce");
		}

		if (pack) {
			pack	=	false;
			Pack();

			Debug.Log("готово pack");
		}

		if (show) {
			show	=	false;
			



		}
    }

	//	получить две строки, вставки и текст
	string[] GetFormsAndText (string _text)
	{
		//	получить массив вставок
		var	arrayForms		=	GetTextForms(_text);

		var forms			=	"";
		var text			=	_text.Replace("\n", "");

		//	если форматирования нет то незачем и страдать
		if (arrayForms.Length >= 0)
		{
			//	объединяю формы через запятую
			forms			=	string.Join(",", arrayForms);
			//	заменяю вставки на разделители
			text			=	string.Join("[-]", GetTextItems(text));
		}

		return new string[]{ forms, text + '.'};
	}

	
	void Pack ()
	{
		var	keys				=	ReadFile(folder + "keys.txt").Split('\n');
		var	forms				=	ReadFile(folder + "forms.txt").Split('\n');
		
		var	texts_en_small		=	GetMergedFiles(folder + "1/small/");
		var	texts_en_large		=	GetMergedFiles(folder + "1/large/");

		var	texts_en_str		=	texts_en_small + "\n" + texts_en_large;
			texts_en_str		=	texts_en_str.Replace("[ -]", "[-]");
			texts_en_str		=	texts_en_str.Replace("[- ]", "[-]");
			texts_en_str		=	texts_en_str.Replace("[ - ]", "[-]");

		var	texts_en			=	texts_en_str.Split('\n');

		var	texts_ru_small		=	GetMergedFiles(folder + "9/small/");
		var	texts_ru_large		=	GetMergedFiles(folder + "9/large/");

		var	texts_ru_str		=	texts_ru_small + "\n" + texts_ru_large;
			texts_ru_str		=	texts_ru_str.Replace("[ -]", "[-]");
			texts_ru_str		=	texts_ru_str.Replace("[- ]", "[-]");
			texts_ru_str		=	texts_ru_str.Replace("[ - ]", "[-]");

		var	texts_ru			=	texts_ru_str.Split('\n');

		Debug.Log("Pack: " + keys.Length + " / " + forms.Length + " / " + texts_en.Length + " / " + texts_ru.Length);

		var file_original		=	ReadFile(folder + fileName).Split('\n');
		var first_line			=	file_original[0];

		var resultGrid			=	CSVReader.SplitCsvGrid(folder + fileName);

		var keys_original		=	new string[resultGrid.GetLength(1)];
		var sorted				=	new string[keys_original.Length];

		
		for (var i=0; i<keys_original.Length; i++)
		{
			keys_original[i]	=	resultGrid[0, i];

			for (var j=0; j<keys.Length; j++)
			{
				if (keys[j].Equals(keys_original[i]))
				{
					//sorted[i]	=	keys[i] + "," + GetText(forms[i], texts_en[i], i) + "" + GetText(forms[i], texts_ru[i], i) + ","
				}
			}
		}
			


		//

		using (CSVWriter writer = new CSVWriter(folder + "translate/" + fileName))
		{
			writer.Write(file_original[0]);

			for (var i=1; i<keys.Length; i++)
			{
				var line	=	new CSVRow();

				for (var j=0; j<max_; j++)
				{
					if (j==0)
						line.Add(keys[i]);
					else 
					if (j==1)
						line.Add(GetText(forms[i], texts_en[i], i));
					else
					if (j==9)
						line.Add(GetText(forms[i], texts_ru[i], i));
					else
						line.Add("");
				}

				writer.WriteRow(line);
			}
		}
	}




	//	получить итоговый текст со скобками и запятыми
	string GetText (string _forms, string _text, int _i=-1)
	{
		if (_text == "")
			return "";

		var result			=	_text;

		//	убрать последнюю точку
		if (result[result.Length-1] == '.')
			result			=	result.Substring(0, result.Length - 1);

		//	если ворматирование отсутствует
		if (_forms == "")
			return result;

		var	arrayForms		=	_forms.Split(',');
		var arrayText		=	Regex.Split(result, @"\[\-\]");

		result				=	"";

		if (arrayText.Length > arrayForms.Length)
		{
			//	те самые магические три строчки что перводят формы и текст в форматированный текст
			for (var i=0; i<arrayForms.Length; i++)
				result		+=	arrayText[i] + arrayForms[i];

			result			+=	arrayText[arrayForms.Length];
		}
		else
		{
			Debug.LogWarning(_i + ") strings_ru : " + " / " + arrayForms.Length + " / " + arrayText.Length + " / " + _forms + " / " + _text) ;

			for (var i=0; i<arrayText.Length; i++)
				result			+=	arrayText[i] + arrayForms[i];

			for (var i=arrayText.Length; i<arrayForms.Length; i++)
				result			+=	arrayForms[i];
		}

		return result;
	}


	void SaveFiles (string _folder, List<string> _strings, int _charsMax=4900)
	{
		ClearFiles(_folder);

		var textFile		=	"";
		var fileIndex		=	0;
		var textList		=	new List<string>();

		var	path			=	_folder + "0.txt";

		for (var i=0; i<_strings.Count; i++)
		{
			if (textFile.Length + _strings[i].Length < (_charsMax-1) ) {
				textList.Add(_strings[i]);
			}
			else
			{
				WriteToFile(path, textFile);

				fileIndex++;

				path			=	_folder + fileIndex + ".txt";

				textList.Clear();
				textList.Add(_strings[i]);
			}

			textFile			=	string.Join("\n", textList);
		}

		//	пишу последнее
		if (textList.Count > 0)
			WriteToFile(path , textFile);
		
	}

	void Parce ()
	{
		var file			=	ReadFile(folder + fileName);

		var formsAll		=	new List<string>();
		var formsSmall		=	new List<string>();
		var formsLarge		=	new List<string>();
			
		var textAll			=	new List<string>();
		var textSmall		=	new List<string>();
		var textLarge		=	new List<string>();

		var keysAll			=	new List<string>();
		var keysLarge		=	new List<string>();
		var keysSmall		=	new List<string>();

		var resultGrid		=	CSVReader.SplitCsvGrid(file);

		for (var i=0; i<resultGrid.GetLength(1); i++)
		{
			if (resultGrid[0, i] == null || resultGrid[0, i].Equals(""))
				continue;

			var formsAndText	=	GetFormsAndText(resultGrid[1, i]);

			var	key				=	resultGrid[0, i];
			var forms			=	formsAndText[0];
			var text			=	formsAndText[1];

			//	чисттка от лишних пробелов и разделение на длинный короткий
			if (text.Split(' ').Length > 1)
			{
				var re				=	@"([ ]+|\[-\]+)+";

				var options			=	RegexOptions.None;
				var regex			=	new Regex(re, options);     

				var textClean		=	regex.Replace(text, " ");
					textClean		=	Regex.Replace(textClean, @"^\s+|\s+$", "");

				var	textWords		=	textClean.Split(' ');

				if (textWords.Length <= 3)
				{
					keysSmall.Add(key);
					formsSmall.Add(forms);
					textSmall.Add(text);
				}
				else
				{
					keysLarge.Add(key);
					formsLarge.Add(forms);
					textLarge.Add(text);
				}
			}
			else
			{
				keysSmall.Add(key);
				formsSmall.Add(forms);
				textSmall.Add(text);
			}
		}

		for (var i=0; i<keysSmall.Count; i++)
		{
			keysAll.Add(keysSmall[i]);
			formsAll.Add(formsSmall[i]);
			textAll.Add(textSmall[i]);
		}

		for (var i=0; i<keysLarge.Count; i++)
		{
			keysAll.Add(keysLarge[i]);
			formsAll.Add(formsLarge[i]);
			textAll.Add(textLarge[i]);
		}

		WriteToFile(folder + "keys.txt" ,  string.Join("\n", keysAll));
		WriteToFile(folder + "forms.txt" , string.Join("\n", formsAll));

		SaveFiles(folder + "1/small/" , textSmall, 4990);
		SaveFiles(folder + "1/large/" , textLarge, 9990);
	}

	void ParceDebug ()
	{
		var file			=	ReadFile(folder + fileName);
		var fileList		=	file.Split('\n');

		var resultGrid		=	CSVReader.SplitCsvGrid(file);
		var	keys			=	new List<string>();
		var	forms			=	new List<string>();
		var	texts			=	new List<string>();
		var	origins			=	new List<string>();

		for (var i=0; i<resultGrid.GetLength(1); i++)
		{
			if (resultGrid[0, i] == null || resultGrid[0, i].Equals(""))
				continue;

			var	key				=	resultGrid[0, i];
			var formsAndText	=	GetFormsAndText(resultGrid[1, i]);

			keys.Add(key);
			forms.Add(formsAndText[0]);
			texts.Add(formsAndText[1]);
			origins.Add(fileList[i]);
		}

		var firstLine		=	fileList[0].Replace("\n", "");

		WriteTranslate(folder + "translate/" + fileName, firstLine, keys, forms, texts, origins);
	}


	void Parce3 ()
	{
		var text			=	"Welcome {PlayerName} to a round of cards!";

		var formsAndText	=	GetFormsAndText(text);

		Debug.Log("forms: " + formsAndText[0]);
		Debug.Log("text: "  + formsAndText[1]);

		var	line			=	GetText(formsAndText[0], formsAndText[1]);

		Debug.Log("line: |"  + line + "|");
	}


	void Pack_test2 ()
	{
		var debug_string	=	"We will play for [-]Gold Coins[-].[-]How many [-]Gold Coins[-] do you want to bet?";
		var debug_array		=	Regex.Split(debug_string, @"\[\-\]");

		//Debug.Log("Pack_test2: " + debug_array.Length + " / " + debug_string);

		for (var i=0; i<debug_array.Length; i++)
			Debug.Log(i + ") " + debug_array[i]);


	}

	void WriteTranslate (string _path, string _firstLine, List<string> _keys, List<string> _forms, List<string> _texts_en, List<string> _texts_original)
	{
		using (CSVWriter writer = new CSVWriter(_path))
		{
			writer.Write(_firstLine);
			writer.WriteLine(_texts_original[0]);

			for (var i=1; i<_keys.Count; i++)
			{
				var line	=	new CSVRow();

				for (var j=0; j<max_; j++)
				{
					if (j==0)
						line.Add(_keys[i]);
					else 
					if (j==1)
						line.Add(GetText(_forms[i], _texts_en[i]));
					else
					if (j==9)
						line.Add("");
					else
						line.Add("");
				}

				writer.WriteRow(line);
				writer.WriteLine(_texts_original[i]);
			}
		}
	}




	string[] GetTextForms (string _text)
	{
		var matches		=	Regex.Matches(_text, sPattern_);
		var array		=	new string[matches.Count];

		for (int i=0; i<matches.Count; i++)
			array[i]	=	matches[i].ToString().Replace("\n", "");

		return array;
	}


	//	буду вызывать тольок когда точно знаю что нет форматирования
	string[] GetTextItems (string _text)
	{
		return GetTextItems(_text, sPattern_);
	}

	string[] GetTextItems (string _text, string _regex)
	{
		var	splitting	=	Regex.Split(_text, _regex);

		var result		=	new string[(splitting.Length-1)/2+1];	
		
		for (var i=0; i<result.Length; i++)
			result[i]	=	splitting[i*2];

		return result;
	}

	

	void CheckFiles (string _path)
	{
		for (var i=0; i<1000; i++)
		{
			var	path_en		=	_path + "1/" + i + ".txt";
			var	path_ru		=	_path + "9/" + i + ".txt";

			if (File.Exists(path_en))
			{
				if (!File.Exists(path_ru))
				{
					Debug.LogError("отсутствует файл перевода, id: " + i);
					break;
				}
				else
				{
					var	file_en				=	ReadFile(path_en).Split('\n');
					var	file_ru				=	ReadFile(path_ru).Split('\n');

					if (file_en.Length != file_ru.Length)
					{ 
						Debug.LogError("не совпадает число строк, id: " + i + ", en: " + file_en.Length + ", ru: " + file_ru.Length);
						//break;
					}
					else
					{
						Debug.Log("файл прошёл проверку, id: " + i + ", en: " + file_en.Length + ", ru: " + file_ru.Length);
					}
				}
			}
			else
			{
				//Debug.Log("файлы кончились id:" + i);

				break;
			}
					
		}
	}

	string GetMergedFiles (string _path)
	{
		var	result		=	"";

		for (var i=0; i<1000; i++)
		{
			var	path_file			=	_path + i + ".txt";

			if (File.Exists(path_file))
				result	+=	(i == 0 ? "" : "\n") + ReadFile(path_file);
			else
				break;
		}

		return result;
	}


	string ReadFile (string _path)
	{
		if (!File.Exists(_path))
			return "";
		else
		{
			var reader	=	new StreamReader(_path); 
			var	result	=	reader.ReadToEnd();
				reader.Close();

			return result;
		}
	}


	string GetCorrectText (string _text)
	{
		if (_text == "")
			return "";

		//	убрать последнюю точку

		if (_text[_text.Length-1] == '.' && _text[_text.Length-2] == ' ')
			_text	=	_text.Substring(0, _text.Length - 2);
		else
		if (_text[_text.Length-1] == '.')
			_text	=	_text.Substring(0, _text.Length - 1);

		return	(_text.Contains("\"") || _text.Contains(",")) ? ('"' + _text + '"') : _text;
	}

	//	получить итоговый текст со скобками и запятыми
	string GetText_old2 (string _forms, string _text)
	{
		if (_text == "")
			return "";

		var result			=	_text;

		//	убрать последнюю точку
		if (result[result.Length-1] == '.')
			result			=	result.Substring(0, result.Length - 1);

		//	если ворматирование отсутствует
		if (_forms == "")
			return result;

		//	на всякий пожарный удаляю закрывающую скобочку
		result				=	result.Replace("[-]", "[");

		var	arrayForms		=	_forms.Split(',');
		var arrayText		=	result.Split('[');

		result				=	"";

		if (arrayText.Length > arrayForms.Length)
		{
			//	те самые магические три строчки что перводят формы и текст в форматированный текст
			for (var i=0; i<arrayForms.Length; i++)
				result			+=	arrayText[i] + arrayForms[i];

			result				+=	arrayText[arrayForms.Length];
		}
		else
		{
			Debug.LogWarning("strings_ru : " + " / " + arrayForms.Length + " / " + arrayText.Length + " / " + _forms + " / " + _text) ;

			for (var i=0; i<arrayText.Length; i++)
				result			+=	arrayText[i] + arrayForms[i];

			for (var i=arrayText.Length; i<arrayForms.Length; i++)
				result			+=	arrayForms[i];
		}

		return result;
	}


	//	удаляем ненужные файлы
	void ClearFiles (string _folder)
	{
		for (var i=0; i<1000; i++)
		{
			var filePath	=	_folder + i + ".txt";

			if (File.Exists(filePath))
				File.Delete(filePath);
			//else
				//break;
		}
	}

	void WriteToFiles (string _folder, int _charsLimit, List<string> _strings, List<string> _keys, List<string> _forms)
	{
		ClearFiles(_folder + "1/");

		var	allStrings			=	string.Join(".\n", _strings);
		var	allKeys				=	string.Join("\n", _keys);
		var	allForms			=	string.Join("\n", _forms);

		Debug.Log("WriteToFiles " + "Assets/" + _folder + " : " + _strings.Count + ", длинна символов: " + allStrings.Length);

		WriteToFile("Assets/" + _folder + "all.txt",  allStrings);
		WriteToFile("Assets/" + _folder + "keys.txt", allKeys);
		WriteToFile("Assets/" + _folder + "forms.txt", allForms);

		var	textResult			=	"";
		var	textList			=	new List<string>();
		var	fileIndex			=	0;

		string filePath;

		for (var i=0; i<_strings.Count; i++)
		{
			if (textResult.Length + _strings[i].Length < (_charsLimit-1) )
			{
				textList.Add(_strings[i]);
				textResult		=	string.Join(".\n", textList);
			}
			else
			{
				filePath	=	"Assets/" + _folder + "1/" + fileIndex + ".txt";

				WriteToFile(filePath , textResult);

				fileIndex++;

				textList.Clear();
				textList.Add(_strings[i]);

				textResult	=	_strings[i];
			}
		}

		filePath	=	"Assets/" + _folder + "1/" + fileIndex + ".txt";

		WriteToFile(filePath , textResult);

	}

	void WriteToFile (string _path, string _text)
	{
		//Debug.Log("WriteToFile: " + _path );

		if (!File.Exists(_path))
#pragma warning disable CS0642 // Возможно, ошибочный пустой оператор
			using (File.CreateText(_path));
#pragma warning restore CS0642 // Возможно, ошибочный пустой оператор

		var writer = new StreamWriter(_path, false, System.Text.Encoding.UTF8);
			writer.Write(_text);
			writer.Close();
	}

}
