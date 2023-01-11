[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/licenses/MIT)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/MGSV_SaveTranslator?label=version)](https://github.com/mi5hmash/MGSV_SaveTranslator/releases/latest)
[![Visual Studio 2022](https://img.shields.io/badge/VS%202022-blueviolet?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)
[![dotNET7](https://img.shields.io/badge/.NET%207-blueviolet)](https://visualstudio.microsoft.com/)

# :gear: MGSV_SaveTranslator - What is it :interrobang:

<img src="https://github.com/mi5hmash/MGSV_SaveTranslator/blob/main/.resources/images/Logo.png" alt="Logo"/>

This app can **decrypt and encrypt SaveData files** from the **Metal Gear Solid V: The Phantom Pain** & **Ground Zeroes** games.
It is useful during **changing the region of the SaveData or porting it to another gaming platform.**

| Game Title | Tested Version |
|------------|----------------|
| MGSV:TPP   | 1.0.15.3       |
| MGSV:GZ*    | 1.0.0.5        |

\* No cross-platform compatibility.

| Supported formats      |
|------------------------|
| PC                     |
| PS4 (decrypted saves)* |

*\* SaveData files on consoles are additionally packed with a method specific to a given console. **This tool doesn't support any of these methods.***

# 🤯 Why was it created :interrobang:
I just wanted to continue playing on SteamDeck from where I left off on PS4 console.

# :scream: Is it safe?
**No.** You can corrupt your SaveData files and lose your progress or get banned from playing online if you unreasonably modify your file.

**Remember to** always make a backup of the files you are about to modify.

With that being said, let's continue to the next chapter where you will learn about the features of this tool.

# :scroll: How to use this tool

<img src="https://github.com/mi5hmash/MGSV_SaveTranslator/blob/main/.resources/images/TranslatorPage.png" alt="TranslatorPage"/>

Let's assume you want to translate SaveData files from PS4 to PC and you have a decrypted and unpacked PS4 save archive.

**List of filenames you can expect:**
* **MGO_GAME_DATA0**
* **MGO_GAME_DATA1**
* **PERSONAL_DATA0**
* **PERSONAL_DATA1**
* **TPP_CONFIG_DATA0**
* **TPP_CONFIG_DATA1**
* **TPP_GAME_DATA0**
* **TPP_GAME_DATA1**

You have to fill in the TextBox **(1)** by dragging and dropping the file on it, pasting the path to that file, or selecting the file using the FilePicker by clicking on the button **(2)**.

Once done, you can click the **(3)** button to analyze the file and automatically detect which of the already defined profiles **(4)** should match that case. If the file is already decrypted it should work with any profile.
It is possible that a suitable profile would not get found. In that case, you could try to use the Research Page to find the decryption key, but more on that later. If everything went well, button **(5)** will be unlocked. Now all that's left to do is click on the unlocked button, change the profile to ***"[PC] <name_of_the_file_you're_working_on>"***, and click the button again to encrypt the file.

If checkbox **(6)** is checked, after each operation a backup will be created with the extensions ***".bakencr"*** for the encrypted file and ***".bakdecr"*** for decrypted one. 

Button **(7)** does exactly what its name says.

<img src="https://github.com/mi5hmash/MGSV_SaveTranslator/blob/main/.resources/images/ResearchPage.png" alt="ResearchPage"/>

Let's assume the decrypted PS4 SaveData files come from an encrypted archive called ***"MGSVTPPSaveDataJP"***. There is no such profile out of the box, so you have to find a decryption key that will unlock all the files inside it.

First, in the Research tab, fill in the TextBox **(1)** by dragging and dropping the file on it, pasting the path to that file, or selecting the file using the FilePicker by clicking on the button **(2)**.

Now there are 2 possible ways. The key to the files is the first 4 bytes of the MD5 checksum calculated from the file name or the name of the parent archive. If you spelled it right and it's its real name a **(5)** button should generate a proper key super duper fast. Otherwise, you can use the brute-force method under the **(6)** button, which will go through all possible combinations (4,294,967,295) to find the one that matches.
If none of the above methods work, it means that the encryption method has changed.

However, if you were successful, you can add your find to the list of profiles. Replace ***"PLATFORM_NAME"*** inside the **(3)** field with the name of the platform from which the analyzed file comes and click on button **(7)** to add and button **(10)** to save changes. Button **(9)** reloads the last saved profile list. Button **(8)** removes the currently picked profile **(3)** from the profile list.

## Location of the PC SaveData files
On the PC platform saves lives there: 

***<Steam_Installation_Folder>*\userdata\\*<YOUR_STEAM_ID_32>*\287700\local

and

***<Steam_Installation_Folder>*\userdata\\*<YOUR_STEAM_ID_32>*\311340\remote

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issue (hope you won't) then please, feel free to report it [there](https://github.com/mi5hmash/MGSV_SaveTranslator/issues).
# :star: Sources:
* https://github.com/lepoco/wpfui
* https://github.com/CommunityToolkit/dotnet
