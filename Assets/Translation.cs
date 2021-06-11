using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]

public class Translation : MonoBehaviour
{
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
	public	bool	translate;

	//public	bool	c;

	public	bool	checkFiles;

	public	bool	pack;

	//public TextAsset	pda_original;

	public	int			smallNum		=	2;

	public	int			rus				=	9;

	int		max_	=	18;

    //
	void Update()
	{
		if (parce) {
			parce	=	false;
			Parce();
		}

		/*
        if (debugTranslate) {
			debugTranslate	=	false;

			var	forms				=	ReadFile("Assets/large/forms.txt").Split('\n');
				
			var	fileStrings			=	ReadFile("Assets/large/1/0.txt");

			//Debug.Log("debugTranslate: " + strings);

			//var	chars			=	new char[]{'.', '\n'};

			//var	file0Array		=	file0String.Split(chars, System.StringSplitOptions.RemoveEmptyEntries);

			var	fileArray			=	fileStrings.Split('\n');

			Debug.Log("debugTranslate: " + forms.Length + " / " + fileArray.Length);

			for (var i=0; i<fileArray.Length; i++)
			{
				//var	removepoint	=	file0Array[i].

				var	text		=	GetCorrectText(fileArray[i]);
				var	str			=	GetText(text, forms[i], i);
			}
		}
		*/

		if (checkFiles) {
			checkFiles	=	false;
			CheckFiles();
		}

		if (translate) {
			translate	=	false;

			var	file_large_merged		=	GetMergedFiles("Assets/large/1/").Split('\n');
			var	file_large_merged_ru	=	GetMergedFiles("Assets/large/9/").Split('\n');
			var	file_large_all			=	ReadFile("Assets/large/all.txt").Split('\n');

			Debug.Log("translate large: " + file_large_merged.Length + " / " + file_large_all.Length);

			var	file_small_merged		=	GetMergedFiles("Assets/small/1/").Split('\n');
			var	file_small_merged_ru	=	GetMergedFiles("Assets/small/9/").Split('\n');
			var	file_small_all			=	ReadFile("Assets/small/all.txt").Split('\n');

			Debug.Log("translate small: " + file_small_merged.Length + " / " + file_small_all.Length);


			var	file_all				=	new List<string>();
				file_all.AddRange(file_large_all);
				file_all.AddRange(file_small_all);

			var	file_merged				=	new List<string>();
				file_merged.AddRange(file_large_merged);
				file_merged.AddRange(file_small_merged);

			var	file_merged_ru			=	new List<string>();
				file_merged_ru.AddRange(file_large_merged_ru);
				file_merged_ru.AddRange(file_small_merged_ru);

			var	file_original		=	ReadFile("Assets/PDA.csv").Split('\n');

			Debug.Log("translate: " + file_original.Length + " / " + file_all.Count + " / " + file_merged.Count + " / " + file_merged_ru.Count);

			var merged_large		=	string.Join("\n", file_merged);

			WriteToFile("Assets/large/merged.txt", merged_large);


			Debug.Log("length: orig: " + 
				string.Join("", file_original).Length + " / all: " + 
				string.Join("", file_all).Length + " / m: " + 
				string.Join("", file_merged).Length + " / ru: " + 
				string.Join("", file_merged_ru).Length );

		}

		if (pack) {
			pack	=	false;

			var	strings_en	=	GetLanguageStrings(1);

			//Debug.Log("strings_en: " + strings_en.Count) ;

			for (var i=0; i<strings_en.Count; i++)
				Debug.Log("en: " + strings_en[i]);


			//var	strings_ru	=	GetLanguageStrings(9);

			//Debug.Log("strings_en: " + strings_en.Count + ", strings_ru: " + strings_ru.Count) ;
		}
    }


	string[] GetKeys ()
	{
		return ReadFile("Assets/large/keys.txt").Split('\n');
	}

	List<string> GetLanguageStrings (int _lang=9)
	{
		var	path_assets		=	"Assets/";
		var	path_large		=	path_assets + "large/";
		var	path_small		=	path_assets + "small/";

		var	merged_large	=	GetMergedFiles(path_large + _lang + "/");
		var	merged_small	=	GetMergedFiles(path_small + _lang + "/");
		var	merged			=	(merged_large + "\n" + merged_small).Split('\n');

		var	keys_large		=	ReadFile(path_large + "/keys.txt");
		var	keys_small		=	ReadFile(path_small + "/keys.txt");
		var	keys			=	(keys_large + "\n" + keys_small).Split('\n');

		var	forms_large		=	ReadFile(path_large + "/forms.txt");
		var	forms_small		=	ReadFile(path_small + "/forms.txt");
		var	forms			=	(forms_large + "\n" + forms_small).Split('\n');

		var result			=	new List<string>();

		for (var i=0; i<merged.Length; i++)
			result.Add(GetText(merged[i], forms[i]));

		return result;
	}

	void Parce ()
	{
		var	segmentsStrings		=	ReadFile("Assets/PDA.csv").Split('\n');

		Debug.Log("число строк [ " + segmentsStrings.Length + " ]");

		ClearFiles("");
			
		var	smallSegments		=	new List<Segment>();		//	список 
		var	smallKeys			=	new List<string>();			//	список 
		var	smallStrings		=	new List<string>();			//	список 
		var	smallForms			=	new List<string>();			//	список 

		var	largeSegments		=	new List<Segment>();		//	список 
		var	largeKeys			=	new List<string>();			//	список 
		var	largeStrings		=	new List<string>();			//	список 
		var	largeForms			=	new List<string>();			//	список 

		for (var i=0; i<segmentsStrings.Length; i++)
		{
			var	strings			=	GetStrings(segmentsStrings[i], i);

			//	где то ошибка - выхожу
			if (strings == null || strings.Length == 0)
			{
				Debug.LogError("ОШИБКА в строчке [ " + (i+1) + " ]");
				return;
			}

			var	segment			=	new Segment(strings[1], strings[0]);

			var	words			=	segment.text.Split(' ');

			if (words.Length <= smallNum)
			{
				smallSegments.Add(segment);
				smallKeys.Add(segment.key);
				smallStrings.Add(segment.text);
				smallForms.Add(string.Join("\"", segment.forms));
			}
			else
			{
				largeSegments.Add(segment);
				largeKeys.Add(segment.key);
				largeStrings.Add(segment.text);
				largeForms.Add(string.Join("\"", segment.forms));
			}	
		}

		WriteToFiles("small/", 5000-10, smallStrings, smallKeys, smallForms);
		WriteToFiles("large/", 10000-10, largeStrings, largeKeys, largeForms);

		Debug.Log("ОХУЕТЬ!! ГОТОВО!");
	}

	void CheckFiles ()
	{
		Debug.Log("проверяю файл: Assets/small/ -----------------------------");
		CheckFiles("Assets/small/");

		Debug.Log("проверяю файл: Assets/large/ -----------------------------");
		CheckFiles("Assets/large/");
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
	string GetText (string _text, string _forms)
	{
		if (_text == "")
			return "";

		var result			=	_text;

		//	убрать последнюю точку
		if (result[result.Length-1] == '.')
			result			=	result.Substring(0, result.Length - 1);
		
		//	если есть кавычка или запятая то оборачиваю в кавычки
		if (result.Contains("\"") || result.Contains(","))
			result			=	'"' + result + '"';

		//	если ворматирование отсутствует
		if (_forms == "")
			return result;

		//	на всякий пожарный удаляю закрывающую скобочку
		result				=	result.Replace("]", "");

		var	arrayForms		=	_forms.Split('"');
		var arrayText		=	result.Split('[');

		result				=	"";

		//	те самые магические три строчки что перводят формы и текст в форматированный текст
		for (var i=0; i<arrayForms.Length; i++)
			result			+=	arrayText[i] + arrayForms[i];

		result				+=	arrayText[arrayForms.Length];

		//	возвращаю \n
		result				=	result.Replace("[n]", "\\n");
		result				=	result.Replace("[n2]", "\\n\\n");
		result				=	result.Replace("[n3]", "\\n\\n\\n");

		return result;
	}


	//	удаляем ненужные файлы
	void ClearFiles (string _folder)
	{
		for (var i=0; i<1000; i++)
		{
			var filePath	=	"Assets/" + _folder + i + ".txt";

			Debug.Log("clear file: " + filePath);

			if (File.Exists(filePath))
				File.Delete (filePath);
			else
				break;
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
		if (!File.Exists(_path))
#pragma warning disable CS0642 // Возможно, ошибочный пустой оператор
			using (File.CreateText(_path));
#pragma warning restore CS0642 // Возможно, ошибочный пустой оператор

		var writer = new StreamWriter(_path, false, System.Text.Encoding.UTF8);
			writer.Write(_text);
			writer.Close();
	}

	string[] GetStrings (string _string, int _index)
    {
		//	скобочки нужны для других скобочек или запятых

		var	result				=	new string[max_];
		var	isSkobka			=	false;
		var	index				=	0;

		for (var i=0; i<_string.Length; i++)
		{
			//	последний символ
			var isLast			=	i == _string.Length-1;

			//	начало
			if (result[index] == null || result[index].Length == 0)
			{
				result[index]	=	"";

				//	начал со скобки
				if (_string[i] == '"')
				{
					if (isLast || _string[i+1] == ',')
					{
						Debug.LogError("ошибка в строке: [ " + _index + " ] какойто пиздец со скобкой 1");
						return null;
					}

					if (_string[i+1] == '"')
					{
						result[index]	+=	"\"\"";
						continue;
					}

					//	начинаю со скобки
					//result[index]	+=	"\"";
					isSkobka		=	true;
					continue;
				}
			}

			if (isSkobka)
			{
				if (isLast && _string[i] != '"')
				{
					Debug.LogError("ошибка в строке: [ " + _index + " ] какойто пиздец со закрывающей скобкой 2");
					return null;
				}

				if (_string[i] == '"')
				{
					//	игнорирую двойные скобки
					if (!isLast && _string[i+1] == '"')
					{
						result[index]	+=	"\"\"";
						i++;
						continue;
					}

					if (!isLast && _string[i+1] != ',')
					{
						Debug.LogError("ошибка в строке: [ " + _index + " ] какойто пиздец со закрывающей скобкой 3");

						return null;
					}
					
					//	скобка является закрывающей

					//result[index]	+=	"\"";

					isSkobka	=	false;

					//	переход на сл элемент
					if (!isLast && _string[i+1] == ',')
					{
						index++;
						i++;
						continue;
					}

				}

				result[index]		+=	_string[i];
			}
			else
			{
				if (_string[i] == ',')
				{
					index++;
					continue;
				}
				else
					result[index]	+=	_string[i];
			}
		}

		//Debug.Log("GetStrings: [ " + _index + " ] " + result[1]);

		return result;
	}


	void Pack ()
	{
		var	original		=	ReadFile("Assets/large/forms.txt").Split('\n');

		var	new_file		=	new List<string>();
			new_file.Add(original[0]);

		for (var i=1; i<original.Length; i++)
		{

		}
	}

}
