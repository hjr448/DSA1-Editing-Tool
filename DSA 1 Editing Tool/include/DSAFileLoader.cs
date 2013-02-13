﻿using System;
using System.Collections.Generic;
using System.Text;

using DSA_1_Editing_Tool.File_Loader;
using System.IO;
using System.Drawing;

using System.Windows.Forms;

namespace DSA_1_Editing_Tool
{
    public enum DSAVersion { Unbekannt, Schick, Blade, Schweif }

    public class CDSAFileLoader
    {
        public CDSAFileLoader()
        {
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.STAR));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.SPEECH));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.RAW));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.FX));

            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.CD_STARCD));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.CD_STAR));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.CD_SPEECHCD));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.CD_SPEECH));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.CD_RAW));
            this.DSA2_Archive.Add(new DSA2_Archiv(DSA2_Archiv.ArchivTyp.CD_FX));
        }
        //-----------DSA 1----------------------
        List<string> itsSCHICKFilenames = new List<string>();
        List<Int32> itsSCHICKOffsets = new List<Int32>();

        List<KeyValuePair<string, Int32>> itsDSAGENOffsets = new List<KeyValuePair<string, Int32>>();

        byte[] SCHICK_DAT = null;
        byte[] DSAGEN_DAT = null;

        //-----------DSA 2----------------------
        //byte[] DSA2_STAR_DAT = null;

        //List<KeyValuePair<string, Int32>> itsSTAROffsets = new List<KeyValuePair<string, Int32>>();
        List<DSA2_Archiv> DSA2_Archive = new List<DSA2_Archiv>();

        //--------------------------------------

        private DSAVersion _version = DSAVersion.Unbekannt;
        public DSAVersion Version { get { return this._version; } }

        // zum auslesen der einzelnen Dateien
        public CItemList itemList = new CItemList();
        public CTextList texte = new CTextList();
        public CMonster monster = new CMonster();
        public CKampf kampf = new CKampf();
        public CStädte städte = new CStädte();
        public CDungeon_list dungeons = new CDungeon_list();
        public CBilder bilder = new CBilder();
        public CRouten routen = new CRouten();
        public CDialoge dialoge = new CDialoge();

        public class CFileSet
        {
            public Int32 startOffset = 0;
            public Int32 endOffset = 0;
            public string filename = "";

            public CFileSet(string filename, Int32 startOffset, Int32 endOffset)
            {
                this.filename = filename;
                this.startOffset = startOffset;
                this.endOffset = endOffset;
            }
        }

        public bool loadFiles(string filepath)
        {
            if (!this.unpackAll(filepath))
                return false;

            switch (this._version)
            {
                case DSAVersion.Schick:
                case DSAVersion.Blade:
                    this.loadDSA1();
                    break;

                case DSAVersion.Schweif:
                    this.loadDSA2(filepath);
                    break;

                default:
                    return false;
            }

            return true;
        }
        private void loadDSA1()
        {
            this.clearItems();

            CFileSet fileset_1;
            CFileSet fileset_2;
            CFileSet fileset_3;
            List<CFileSet> filesetList_1;
            List<CFileSet> filesetList_2;

            if (Properties.Settings.Default.loadData)
            {
                fileset_1 = this.getFileByName_DSA_1("ITEMS.DAT", false);
                fileset_2 = this.getFileByName_DSA_1("ITEMNAME", false);
                this.itemList.addItems(ref this.SCHICK_DAT, fileset_1, fileset_2, this._version);

                filesetList_1 = this.getFilesBySuffix_DSA_1(".LTX", false);
                filesetList_2 = this.getFilesBySuffix_DSA_1(".DTX", false);
                this.texte.addTexte(ref this.SCHICK_DAT, filesetList_1, filesetList_2);

                fileset_1 = this.getFileByName_DSA_1("MONSTER.DAT", false);
                fileset_2 = this.getFileByName_DSA_1("MONNAMES", false);
                this.monster.addMonsters(ref this.SCHICK_DAT, fileset_1, fileset_2, this._version);

                fileset_1 = this.getFileByName_DSA_1("FIGHT.LST", false);
                this.kampf.addKämpfe(ref this.SCHICK_DAT, fileset_1);

                filesetList_1 = this.getTownFiles();
                this.städte.addStädte(ref this.SCHICK_DAT, filesetList_1);

                filesetList_1 = this.getFilesBySuffix_DSA_1(".DNG", false);
                filesetList_2 = this.getFilesBySuffix_DSA_1(".DDT", false);
                this.dungeons.addDungeons(ref this.SCHICK_DAT, filesetList_1, filesetList_2);
            }

            if (Properties.Settings.Default.loadImages)
            {
                filesetList_1 = this.getFilesBySuffix_DSA_1(".NVF", false);
                filesetList_2 = this.getFilesBySuffix_DSA_1(".NVF", true);
                this.bilder.addPictures(ref this.SCHICK_DAT, filesetList_1, ref this.DSAGEN_DAT, filesetList_2, this.Version);
                //-------------Main Images------------------
                CDebugger.addDebugLine("weitere Bilder werden geladen, bitte warten...");

                fileset_1 = this.getFileByName_DSA_1("COMPASS", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("SPLASHES.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("TEMPICON", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KARTE.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("BICONS", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("ICONS", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);

                //-------------Main Power Pack--------------
                fileset_1 = this.getFileByName_DSA_1("PLAYM_UK", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("PLAYM_US", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("ZUSTA_UK", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("ZUSTA_US", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);

                fileset_1 = this.getFileByName_DSA_1("BUCH.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KCBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KCLBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KDBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KDLBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KLBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KLLBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KSBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("KSLBACK.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);

                //fileset_1 = this.getFileByName("BSKILLS.DAT", false);
                //this.bilder.addPictureToList(ref this.MAIN_DAT, fileset_1);

                fileset_1 = this.getFileByName_DSA_1("POPUP.DAT", false);
                this.bilder.addPictureToList(ref this.SCHICK_DAT, fileset_1, this.Version);
                //-------------DSA GEN Images---------------
                fileset_1 = this.getFileByName_DSA_1("ATTIC", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DSALOGO.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("GENTIT.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("HEADS.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version); //daten sind amiga komprimiert
                fileset_1 = this.getFileByName_DSA_1("SEX.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                //-------------DSA GEN Power Pack------------
                fileset_1 = this.getFileByName_DSA_1("DZWERG.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DTHORWAL.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DSTREUNE.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DMENGE.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DMAGIER.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DKRIEGER.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DDRUIDE.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DAELF.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DFELF.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DWELF.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DGAUKLER.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DHEXE.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("DJAEGER.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);

                fileset_1 = this.getFileByName_DSA_1("POPUP.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
                fileset_1 = this.getFileByName_DSA_1("ROALOGUS.DAT", true);
                this.bilder.addPictureToList(ref this.DSAGEN_DAT, fileset_1, this.Version);
            }

            if (Properties.Settings.Default.loadAnims)
            {
                //---------Bild Archive-----------------------
                fileset_1 = this.getFileByName_DSA_1("MONSTER", false);
                fileset_2 = this.getFileByName_DSA_1("MONSTER.TAB", false);
                this.bilder.addArchivToList(ref this.SCHICK_DAT, fileset_1, fileset_2, this._version);
                fileset_1 = this.getFileByName_DSA_1("MFIGS", false);
                fileset_2 = this.getFileByName_DSA_1("MFIGS.TAB", false);
                this.bilder.addArchivToList(ref this.SCHICK_DAT, fileset_1, fileset_2, this._version);
                fileset_1 = this.getFileByName_DSA_1("WFIGS", false);
                fileset_2 = this.getFileByName_DSA_1("WFIGS.TAB", false);
                this.bilder.addArchivToList(ref this.SCHICK_DAT, fileset_1, fileset_2, this._version);

                //Bilder im Archiv ANIS sind in einem alten Animationsformat (siehe wiki)
                fileset_1 = this.getFileByName_DSA_1("ANIS", false);
                fileset_2 = this.getFileByName_DSA_1("ANIS.TAB", false);
                this.bilder.addArchivToList(ref this.SCHICK_DAT, fileset_1, fileset_2, this._version);
            }

            int fileCount;
            int imageCount;
            this.bilder.getImageCount(out fileCount, out imageCount);
            CDebugger.addDebugLine("es wurden " + fileCount.ToString() + " Bilddatein mit insgesammt " + imageCount.ToString() + " Bildern geladen");

            //----------Routen--------------------
            fileset_1 = this.getFileByName_DSA_1("LROUT.DAT", false);
            fileset_2 = this.getFileByName_DSA_1("HSROUT.DAT", false);
            fileset_3 = this.getFileByName_DSA_1("SROUT.DAT", false);
            this.routen.addRouten(ref this.SCHICK_DAT, fileset_1, fileset_2, fileset_3);

            //----------Dialoge--------------------
            filesetList_1 = this.getFilesBySuffix_DSA_1("TLK", false);
            this.dialoge.addDialoge(ref this.SCHICK_DAT, filesetList_1);
        }
        private void loadDSA2(string filepath)
        {
            this.clearItems();

            CFileSet fileset_1;
            CFileSet fileset_2;
            CFileSet fileset_3;
            List<CFileSet> filesetList_1;
            List<CFileSet> filesetList_2;

            ///////////////
            //  Texte
            //if (Properties.Settings.Default.loadData)
            //{
            //    string myFile = filepath + Path.DirectorySeparator + "DATA" + Path.DirectorySeparator + "GLOBLTXT.LTX";
            //    if (File.Exists(myFile))
            //    {
            //        byte[] data = File.ReadAllBytes(myFile);
            //        fileset_1 = new CFileSet("GLOBLTXT.LTX", 0, data.Length);
            //        this.texte.addLTX(ref data, fileset_1);
            //    }
            //}

            DSA2_Archiv currentArchiv = null;
            foreach (var Archiv in this.DSA2_Archive)
            {
                if (Archiv.Typ == DSA2_Archiv.ArchivTyp.STAR)
                {
                    currentArchiv = Archiv;
                    break;
                }
            }

            if (currentArchiv != null)
            {
                filesetList_1 = this.getFilesBySuffix_DSA_2("LTX", currentArchiv.Typ);
                filesetList_2 = this.getFilesBySuffix_DSA_2("DTX", currentArchiv.Typ);
                this.texte.addTexte(ref currentArchiv.data, filesetList_1, filesetList_2);
            }

            if (currentArchiv != null)
            {
                fileset_1 = this.getFileByName_DSA_2("ITEMS.DAT", currentArchiv.Typ);
                fileset_2 = this.getFileByName_DSA_2("ITEMS.LTX", currentArchiv.Typ);
                this.itemList.addItems(ref currentArchiv.data, fileset_1, fileset_2, this._version);

                fileset_1 = this.getFileByName_DSA_2("MONSTER.DAT", currentArchiv.Typ);
                fileset_2 = this.getFileByName_DSA_2("MONNAMES.LTX", currentArchiv.Typ);
                this.monster.addMonsters(ref currentArchiv.data, fileset_1, fileset_2, this._version);
            }

            if (currentArchiv != null)  //vielleicht später nochmal die CD miteinbeziehen
            {
                filesetList_1 = this.getFilesBySuffix_DSA_2("NVF", currentArchiv.Typ);
                this.bilder.addPictures(ref currentArchiv.data, filesetList_1, this.Version);
                //---------------------------
            }

        }
        
        public bool unpackAll(string directoryPath)
        {
            CDebugger.clearDebugText();

            this.itsDSAGENOffsets.Clear();
            this.itsSCHICKFilenames.Clear();
            this.itsSCHICKOffsets.Clear();
            this.SCHICK_DAT = null;
            this.DSAGEN_DAT = null;

            foreach (DSA2_Archiv archiv in this.DSA2_Archive)
            {
                archiv.data = null;
                archiv.entries.Clear();
            }
           
            bool found = false;

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(directoryPath));  

            if (files.Count == 0)
            {
                CDebugger.addErrorLine("keine .exe Dateien gefunden");
                return false;
            }
            
            foreach (string filepath in files)
            {
                if (String.Compare("SCHICKM.EXE", Path.GetFileName(filepath), true) == 0)
                {
                    Properties.Settings.Default.DefaultDSAPath = directoryPath;
                    Properties.Settings.Default.Save();

                    CDebugger.addDebugLine("SCHICKM.EXE wurde erkannt ");
                    this.loadFilenames_DSA_1(filepath);   //  dateinamen sind in der exe verankert

                    foreach (string s in files)
                    {
                        if (this.SCHICK_DAT == null && String.Compare("SCHICK.DAT", Path.GetFileName(s), true) == 0)
                            this.unpack_SCHICK(s);
                        if (this.DSAGEN_DAT == null && String.Compare("DSAGEN.DAT", Path.GetFileName(s), true) == 0)
                            this.unpack_DSAGEN(s);
                    }

                    if (this.SCHICK_DAT == null || this.DSAGEN_DAT == null)
                    {
                        CDebugger.addErrorLine("Fehler beim laden der .DAT Dateien");
                        return false;
                    }

                    found = true;
                    this._version = DSAVersion.Schick;
                    break;
                }
                else if (String.Compare("BLADEM.EXE", Path.GetFileName(filepath), true) == 0)
                {
                    Properties.Settings.Default.DefaultDSAPath = directoryPath;
                    Properties.Settings.Default.Save();

                    CDebugger.addDebugLine("BLADEM.EXE wurde erkannt ");
                    this.loadFilenames_DSA_1(filepath);   //  dateinamen sind in der exe verankert

                    foreach (string s in files)
                    {
                        if (this.SCHICK_DAT == null && String.Compare("BLADE.DAT", Path.GetFileName(s), true) == 0)
                            this.unpack_SCHICK(s);

                        if (this.DSAGEN_DAT == null && String.Compare("DSAGEN.DAT", Path.GetFileName(s), true) == 0)
                            this.unpack_DSAGEN(s);
                    }

                    if (this.SCHICK_DAT == null || this.DSAGEN_DAT == null)
                    {
                        CDebugger.addErrorLine("Fehler beim laden der .DAT Dateien");
                        return false;
                    }

                    found = true;
                    this._version = DSAVersion.Blade;
                    break;
                }
                else if (String.Compare("SCHWEIF.EXE", Path.GetFileName(filepath), true) == 0)
                {
                    Properties.Settings.Default.DefaultDSAPath = directoryPath;
                    Properties.Settings.Default.Save();

                    CDebugger.addDebugLine("SCHWEIF.EXE wurde erkannt ");

                    this._version = DSAVersion.Schweif;

                    string[] helper = Directory.GetDirectories(directoryPath);
                    foreach (var s in helper)
                    {
                        if (String.Compare(new DirectoryInfo(s).Name, "DATA", true) == 0)
                        {
                            this.unpack_SCHWEIF(s);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        CDebugger.addErrorLine("DATA Verzeichnis wurde nicht gefunden");
                        return false;
                    }

                    break;
                }
                else if (String.Compare("STAR.EXE", Path.GetFileName(filepath), true) == 0)
                {
                    Properties.Settings.Default.DefaultDSAPath = directoryPath;
                    Properties.Settings.Default.Save();

                    CDebugger.addDebugLine("STAR.EXE wurde erkannt ");

                    this._version = DSAVersion.Schweif;

                    string[] helper = Directory.GetDirectories(directoryPath);
                    foreach (var s in helper)
                    {
                        if (String.Compare(new DirectoryInfo(s).Name, "DATA", true) == 0)
                        {
                            this.unpack_SCHWEIF(s);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        CDebugger.addErrorLine("DATA Verzeichnis wurde nicht gefunden");
                        return false;
                    }

                    break;
                }
            }

            if (!found)
            {
                CDebugger.addErrorLine("DSA Version konnte nicht erkannt werden");
                string[] exe_Files = Directory.GetFiles(directoryPath, "*.exe");
                string[] EXE_Files = Directory.GetFiles(directoryPath, "*.EXE");
                CDebugger.addErrorLine("es wurden " + (exe_Files.Length + EXE_Files.Length).ToString() + " .exe Dateien gefunden:");
                if (exe_Files.Length > 0)
                {
                    foreach (string s in exe_Files)
                        CDebugger.addErrorLine("  - exe: " + s);
                    foreach (string s in EXE_Files)
                        CDebugger.addErrorLine("  - EXE: " + s);
                }
                return false;
            }

            return true;
        }
        public void clearItems()
        {
            this.itemList.clear();
            this.texte.clear();
            this.monster.clear();
            this.kampf.clear();
            this.städte.clear();
            this.dungeons.clear();
            this.bilder.clear();
            this.routen.clear();
            this.dialoge.clear();
        }

        private void loadFilenames_DSA_1(string filename)
        {
            this.itsSCHICKFilenames.Clear();

            if (!File.Exists(filename))
            {
                CDebugger.addErrorLine("die Datei \"" + filename + "\" konnte nicht geladen werden");
                return;
            }

            Byte[] data = File.ReadAllBytes(filename);

            Int32 offset = this.searchInByteStream(ref data, "TEMP\\%s");  //das Zeichen '\' kommt mehrfach vor wegen escapesequenz
            if (offset == -1)
            {
                CDebugger.addErrorLine("der suchterm wurde in \"" + filename + "\" nicht gefunden");
                CDebugger.addErrorLine("--- bitte lade die Datei \"" + filename + "\" im Forum hoch---");
                return;
            }
            offset += 2;    //hier beginnt der erste nullterminierte String

            //endet mit 0x00 0x2E
            while (data[offset] != 0x00 || data[offset + 1] != 0x2E)
            {
                offset++;
                string name = "";
                while (data[offset] != 0x00)
                {
                    name += (char)data[offset];
                    offset++;
                }
                this.itsSCHICKFilenames.Add(name);
            }

            CDebugger.addDebugLine(this.itsSCHICKFilenames.Count.ToString() + " Datei Namen wurden in \"" + filename + "\" gefunden");
        }
        private void unpack_SCHICK(string filename)
        {
            this.itsSCHICKOffsets.Clear();

            if (!File.Exists(filename))
            {
                CDebugger.addErrorLine("die Datei '" + filename + "' konnte nicht geladen werden");
                return;
            }

            this.SCHICK_DAT = File.ReadAllBytes(filename);

            Int32 position = 0;
            Int32 value = 0;

            //offsets auslesen
            while (true)
            {
                value = (Int32)this.SCHICK_DAT[position + 0] + ((Int32)this.SCHICK_DAT[position + 1] << 8) + ((Int32)this.SCHICK_DAT[position + 2] << 16) + ((Int32)this.SCHICK_DAT[position + 3] << 24);
                if (value == 0)
                    break;
                else
                    this.itsSCHICKOffsets.Add(value);

                position += 4;
            }

            CDebugger.addDebugLine(this.itsSCHICKOffsets.Count.ToString() + " Datei Einträge wurden in \"" + filename + "\" gefunden");
        }   
        private void unpack_DSAGEN(string filename)
        {
            this.itsDSAGENOffsets.Clear();

            if (!File.Exists(filename))
            {
                CDebugger.addErrorLine("die Datei '" + filename + "' konnte nicht geladen werden");
                return;
            }

            this.DSAGEN_DAT = File.ReadAllBytes(filename);

            UInt32 nextPosition = 0;
            while (this.DSAGEN_DAT[nextPosition] != 0)
            {
                UInt32 currentPos = nextPosition;

                string name = "";
                while ((this.DSAGEN_DAT[currentPos] != 0) && ((currentPos - nextPosition) < 0x0C))
                {
                    name += (char)this.DSAGEN_DAT[currentPos];
                    currentPos++;
                }

                Int32 value = (Int32)this.DSAGEN_DAT[nextPosition + 0x0C] + ((Int32)this.DSAGEN_DAT[nextPosition + 0x0D] << 8) + ((Int32)this.DSAGEN_DAT[nextPosition + 0x0E] << 16) + ((Int32)this.DSAGEN_DAT[nextPosition + 0x0F] << 24);

                itsDSAGENOffsets.Add(new KeyValuePair<string, Int32>(name, value));

                nextPosition += 16;
            }

            CDebugger.addDebugLine(this.itsDSAGENOffsets.Count.ToString() + " Dateien wurden in \"" + filename + "\" gefunden");
        }
        private void unpack_SCHWEIF(string directoryName)
        {
            bool found = false;
            string CD_Path = null;
            System.IO.DriveInfo[] allDrives = System.IO.DriveInfo.GetDrives();
            
            foreach (System.IO.DriveInfo d in allDrives)
            {
                if (found)
                    break;

                if (d.DriveType == DriveType.CDRom && d.IsReady)
                {
                    foreach (String folder in Directory.GetDirectories(d.RootDirectory.FullName))
                    {
                        if (found)
                            break;

                        if (String.Compare(new DirectoryInfo(folder).Name, "DATA", true) == 0)
                        {
                            foreach (string file in Directory.GetFiles(folder))
                            {
                                if (String.Compare(Path.GetFileName(file), "STAR.DAT", true) == 0)
                                {
                                    CD_Path = folder;
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (CD_Path == null)
                CDebugger.addErrorLine("Sternenschweif CD wurde nicht gefunden");

            //---------------------------------------------------------------------

            foreach (var Archiv in this.DSA2_Archive)
            {
                Archiv.data = null;
                Archiv.entries.Clear();

                string ArchiveName = null;
                bool cd = false;
                switch(Archiv.Typ)
                {
                    case DSA2_Archiv.ArchivTyp.FX:
                        ArchiveName = "FX.DAT";
                        break;
                    case DSA2_Archiv.ArchivTyp.RAW:
                        ArchiveName = "RAW.DAT";
                        break;
                    case DSA2_Archiv.ArchivTyp.SPEECH:
                        ArchiveName = "SPEECH.DAT";
                        break;
                    case DSA2_Archiv.ArchivTyp.STAR:
                        ArchiveName = "STAR.DAT";
                        break;

                    case DSA2_Archiv.ArchivTyp.CD_FX:
                        ArchiveName = "FX.DAT"; cd = true;
                        break;
                    case DSA2_Archiv.ArchivTyp.CD_RAW:
                        ArchiveName = "RAW.DAT"; cd = true;
                        break;
                    case DSA2_Archiv.ArchivTyp.CD_SPEECH:
                        ArchiveName = "SPEECH.DAT"; cd = true;
                        break;
                    case DSA2_Archiv.ArchivTyp.CD_SPEECHCD:
                        ArchiveName = "SPEECHCD.DAT"; cd = true;
                        break;
                    case DSA2_Archiv.ArchivTyp.CD_STAR:
                        ArchiveName = "STAR.DAT"; cd = true;
                        break;
                    case DSA2_Archiv.ArchivTyp.CD_STARCD:
                        ArchiveName = "STARCD.DAT"; cd = true;
                        break;
                }

                string Archive_path = null;
                if (cd && CD_Path != null)
                {
                    foreach (string file in Directory.GetFiles(CD_Path))
                    {
                        if (String.Compare(Path.GetFileName(file), ArchiveName, true) == 0)
                        {
                            Archive_path = file;
                            break;
                        }
                    }
                }
                else if (!cd)
                {
                    foreach (string file in Directory.GetFiles(directoryName))
                    {
                        if (String.Compare(Path.GetFileName(file), ArchiveName, true) == 0)
                        {
                            Archive_path = file;
                            break;
                        }
                    }
                }

                if (Archive_path == null || !File.Exists(Archive_path))
                {
                    if (!(CD_Path == null && cd))
                        CDebugger.addErrorLine("die Datei '" + Archive_path + "' konnte nicht geladen werden");
                    
                    continue;
                }

                Archiv.data = File.ReadAllBytes(Archive_path);


                Int16 anzahlEinträge = CHelpFunctions.byteArrayToInt16(ref Archiv.data, 0);
                Int32 position = 2;
                Int32 offsetFiles = 2 + anzahlEinträge * 20;

                for (int i = 0; i < anzahlEinträge; i++)
                {
                    string name = CHelpFunctions.readDSAString(ref Archiv.data, position + 2, 14);
                    Int32 offset = CHelpFunctions.byteArrayToInt32(ref Archiv.data, position + 16);

                    if (name == "DUMMY")
                    {
                        position += 20;
                        continue;
                    }

                    Int32 value = CHelpFunctions.byteArrayToInt16(ref Archiv.data, position);
                    if ((value == 0) == cd)
                    {
                            Archiv.entries.Add(new KeyValuePair<string, int>(name, offsetFiles + offset));
                    }
                    else if (value > 1)
                        CDebugger.addErrorLine("Die Datei " + name + " ist nicht verfügbar ??? (" + value.ToString() + ")");

                    position += 20;
                }

                CDebugger.addDebugLine(Archiv.entries.Count.ToString() + " Datei Einträge wurden in \"" + Archive_path + "\" gefunden");
            }
        }

        private CFileSet getFileByName_DSA_1(string filename, bool useDSAGenDat)
        {
            if (!useDSAGenDat)
            {
                if (this.SCHICK_DAT != null)
                {
                    for (int i = 0; i < this.itsSCHICKFilenames.Count; i++)
                    {
                        if (this.itsSCHICKFilenames[i] == filename)
                        {
                            //Datei gefunden

                            if (i < (this.itsSCHICKFilenames.Count - 1))
                            {
                                if (i < this.itsSCHICKOffsets.Count)
                                {
                                    return new CFileSet(filename, this.itsSCHICKOffsets[i], this.itsSCHICKOffsets[i + 1]);
                                }
                                else
                                    return null;   //zu dem namen existieren keine bytes
                            }
                            else
                            {
                                if (i < this.itsSCHICKOffsets.Count)
                                {
                                    return new CFileSet(filename, this.itsSCHICKOffsets[i], this.SCHICK_DAT.Length); //datei geht bis zum ende des bytestreams
                                }
                                else
                                    return null;   //zu dem namen existieren keine bytes
                            }
                        }
                    }
                }
            }
            else
            {
                if (this.DSAGEN_DAT != null)
                {
                    for (int i = 0; i < this.itsDSAGENOffsets.Count; i++)
                    {
                        if (this.itsDSAGENOffsets[i].Key == filename)
                        {
                            if (i < (this.itsDSAGENOffsets.Count - 1))
                                return new CFileSet(this.itsDSAGENOffsets[i].Key, this.itsDSAGENOffsets[i].Value, this.itsDSAGENOffsets[i + 1].Value);
                            else
                                return new CFileSet(this.itsDSAGENOffsets[i].Key, this.itsDSAGENOffsets[i].Value, this.DSAGEN_DAT.Length);
                        }
                    }
                }
            }

            return null;
        }
        private CFileSet getFileByName_DSA_2(string filename, DSA2_Archiv.ArchivTyp typ)
        {
            foreach (var Archiv in this.DSA2_Archive)
            {
                if (Archiv.Typ != typ)
                    continue;

                if (Archiv.data == null)
                    return null;

                for (int i = 0; i < Archiv.entries.Count; i++)
                {
                    if (Archiv.entries[i].Key == filename)
                    {
                        if (i < (Archiv.entries.Count - 1))
                            return new CFileSet(filename, Archiv.entries[i].Value, Archiv.entries[i + 1].Value);
                        else
                            return new CFileSet(filename, Archiv.entries[i].Value, Archiv.entries.Count);
                    }
                }
                break;
            }

            return null;
        }
        private List<CFileSet> getFilesBySuffix_DSA_1(string suffix, bool useDSAGenDat)
        {
            List<CFileSet> fileSet = new List<CFileSet>();

            if (!useDSAGenDat)
            {
                if (this.SCHICK_DAT != null)
                {
                    for (int i = 0; i < this.itsSCHICKFilenames.Count; i++)
                    {
                        if (this.itsSCHICKFilenames[i].Contains(suffix))
                        {
                            if (i < (this.itsSCHICKFilenames.Count - 1))
                            {
                                if (i < this.itsSCHICKOffsets.Count)
                                {
                                    fileSet.Add(new CFileSet(this.itsSCHICKFilenames[i], this.itsSCHICKOffsets[i], this.itsSCHICKOffsets[i + 1]));
                                }
                            }
                            else
                            {
                                if (i < this.itsSCHICKOffsets.Count)
                                {
                                    fileSet.Add(new CFileSet(this.itsSCHICKFilenames[i], this.itsSCHICKOffsets[i], this.SCHICK_DAT.Length)); //datei geht bis zum ende des bytestreams
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (this.DSAGEN_DAT != null)
                {
                    for (int i = 0; i < this.itsDSAGENOffsets.Count; i++)
                    {
                        if (this.itsDSAGENOffsets[i].Key.Contains(suffix))
                        {
                            if (i < (this.itsDSAGENOffsets.Count - 1))
                                fileSet.Add(new CFileSet(this.itsDSAGENOffsets[i].Key, this.itsDSAGENOffsets[i].Value, this.itsDSAGENOffsets[i + 1].Value));
                            else
                                fileSet.Add(new CFileSet(this.itsDSAGENOffsets[i].Key, this.itsDSAGENOffsets[i].Value, this.DSAGEN_DAT.Length));
                        }
                    }
                }
            }

            return fileSet;
        }
        private List<CFileSet> getFilesBySuffix_DSA_2(string suffix, DSA2_Archiv.ArchivTyp typ)
        {
            List<CFileSet> fileSet = new List<CFileSet>();

            foreach (var Archiv in this.DSA2_Archive)
            {
                if (Archiv.Typ != typ)
                    continue;

                if (Archiv.data == null)
                    return fileSet;

                for (int i = 0; i < Archiv.entries.Count; i++)
                {
                    if (Archiv.entries[i].Key.Contains(suffix))
                    {
                        if (i < (Archiv.entries.Count - 1))
                            fileSet.Add(new CFileSet(Archiv.entries[i].Key, Archiv.entries[i].Value, Archiv.entries[i + 1].Value));
                        else
                            fileSet.Add(new CFileSet(Archiv.entries[i].Key, Archiv.entries[i].Value, Archiv.entries.Count));
                    }
                }

                break;
            }

            return fileSet;
        }
        private List<CFileSet> getTownFiles()
        {
            string suffix = ".DAT";
            string[] citys = { "THORWAL.DAT", "SERSKE.DAT", "BREIDA.DAT", "PEILINEN.DAT", "ROVAMUND.DAT", "NORDVEST.DAT",
                             "KRAVIK.DAT", "SKELELLE.DAT", "MERSKE.DAT", "EFFERDUN.DAT", "TJOILA.DAT", "RUKIAN.DAT",
                             "ANGBODIRTAL.DAT", "AUPLOG.DAT", "VILNHEIM.DAT", "BODON.DAT", "OBERORKEN.DAT", "PHEXCAER.DAT",
                             "GROENVEL.DAT", "FELSTEYN.DAT", "EINSIEDL.DAT", "ORKANGER.DAT", "CLANEGH.DAT", "LISKOR.DAT",
                             "THOSS.DAT", "TJANSET.DAT", "ALA.DAT", "ORVIL.DAT", "OVERTHORN.DAT", "ROVIK.DAT", "HJALSING.DAT",
                             "GUDDASUN.DAT", "KORD.DAT", "TREBAN.DAT", "ARYN.DAT", "RUNINSHA.DAT", "OTTARJE.DAT", "SKJAL.DAT",
                             "PREM.DAT", "DASPOTA.DAT", "RYBON.DAT", "LJASDAHL.DAT", "VARNHEIM.DAT", "VAERMHAG.DAT", "TYLDON.DAT",
                             "VIDSAND.DAT", "BRENDHIL.DAT", "MANRIN.DAT", "FTJOILA.DAT", "FANGBODI.DAT", "HJALLAND.DAT", "RUNIN.DAT"};

            List<CFileSet> fileSet = new List<CFileSet>();

            if (this.SCHICK_DAT != null)
            {
                for (int i = 0; i < this.itsSCHICKFilenames.Count; i++)
                {
                    if (this.itsSCHICKFilenames[i].Contains(suffix))
                    {
                        bool cityFound = false;
                        foreach (string s in citys)
                        {
                            if (s == this.itsSCHICKFilenames[i])
                            {
                                cityFound = true;
                                break;
                            }
                        }

                        if (!cityFound)
                            continue;

                        if (i < (this.itsSCHICKFilenames.Count - 1))
                        {
                            if (i < this.itsSCHICKOffsets.Count)
                            {
                                fileSet.Add(new CFileSet(this.itsSCHICKFilenames[i], this.itsSCHICKOffsets[i], this.itsSCHICKOffsets[i + 1]));
                            }
                        }
                        else
                        {
                            if (i < this.itsSCHICKOffsets.Count)
                            {
                                fileSet.Add(new CFileSet(this.itsSCHICKFilenames[i], this.itsSCHICKOffsets[i], this.SCHICK_DAT.Length)); //datei geht bis zum ende des bytestreams
                            }
                        }
                    }
                }
            }

            
            return fileSet;
        }

        private Int32 searchInByteStream(ref byte[] data, string searchTerm)
        {
            int position = 0;
            for (Int32 i = 0; i < data.Length; i++)
            {
                if (data[i] == searchTerm[position])
                {
                    position++;
                    if (position >= searchTerm.Length)
                        return i;
                }
                else
                    position = 0;
            }
            return -1;  //suchtext wurde nicht gefunden
        }

        ////////////////
        //   export   //
        ////////////////

        public void exportFiles(string filepath)
        {
            switch (this._version)
            {
                case DSAVersion.Blade:
                case DSAVersion.Schick:
                    if (!Directory.Exists(filepath + Path.DirectorySeparatorChar + "SCHICK"))
                        Directory.CreateDirectory(filepath + Path.DirectorySeparatorChar + "SCHICK");

                    for (int i = 0; i < this.itsSCHICKOffsets.Count; i++)
                    {
                        string typ;
                        if (i < this.itsSCHICKFilenames.Count)
                            typ = getFileTyp(this.itsSCHICKFilenames[i]);
                        else
                            typ = "unbekannt";

                        string name;
                        if (i < this.itsSCHICKFilenames.Count && this.itsSCHICKFilenames[i] != "")
                            name = this.itsSCHICKFilenames[i];
                        else
                            name = i.ToString();

                        if (!Directory.Exists(filepath + Path.DirectorySeparatorChar + "SCHICK" + Path.DirectorySeparatorChar + typ))
                            Directory.CreateDirectory(filepath + Path.DirectorySeparatorChar + "SCHICK" + Path.DirectorySeparatorChar + typ);

                        int end;
                        if (i >= (this.itsSCHICKOffsets.Count - 1))
                            end = this.SCHICK_DAT.Length;
                        else
                            end = this.itsSCHICKOffsets[i + 1];

                        byte[] data = new byte[end - this.itsSCHICKOffsets[i]];
                        Array.Copy(this.SCHICK_DAT, this.itsSCHICKOffsets[i], data, 0, data.Length);

                        if (data.Length > 0)
                            File.WriteAllBytes(filepath + Path.DirectorySeparatorChar + "SCHICK" + Path.DirectorySeparatorChar + typ + Path.DirectorySeparatorChar + name, data);
                    }

                    //----------------------------------------------------

                    if (!Directory.Exists(filepath + Path.DirectorySeparatorChar + "DSAGEN"))
                        Directory.CreateDirectory(filepath + Path.DirectorySeparatorChar + "DSAGEN");

                    for (int i = 0; i < this.itsDSAGENOffsets.Count; i++)
                    {
                        string typ = getFileTyp(this.itsDSAGENOffsets[i].Key);

                        if (!Directory.Exists(filepath + Path.DirectorySeparatorChar + "DSAGEN" + Path.DirectorySeparatorChar + typ))
                            Directory.CreateDirectory(filepath + Path.DirectorySeparatorChar + "DSAGEN" + Path.DirectorySeparatorChar + typ);

                        int end;
                        if (i >= (this.itsDSAGENOffsets.Count - 1))
                            end = this.DSAGEN_DAT.Length;
                        else
                            end = this.itsDSAGENOffsets[i + 1].Value;

                        string name;
                        if (i < this.itsDSAGENOffsets.Count && this.itsDSAGENOffsets[i].Key != "")
                            name = this.itsDSAGENOffsets[i].Key;
                        else
                            name = i.ToString();

                        byte[] data = new byte[end - this.itsDSAGENOffsets[i].Value];
                        Array.Copy(this.DSAGEN_DAT, this.itsDSAGENOffsets[i].Value, data, 0, data.Length);

                        if (data.Length > 0)
                            File.WriteAllBytes(filepath + Path.DirectorySeparatorChar + "DSAGEN" + Path.DirectorySeparatorChar + typ + Path.DirectorySeparatorChar + name, data);
                    }
                    break;

                case DSAVersion.Schweif:
                    foreach (var Archiv in this.DSA2_Archive)
                    {
                        string folder = null;
                        switch(Archiv.Typ)
                        {
                            case DSA2_Archiv.ArchivTyp.FX:
                                folder = filepath + Path.DirectorySeparatorChar + "SCHWEIF" + Path.DirectorySeparatorChar + "FX" ; break;
                            case DSA2_Archiv.ArchivTyp.RAW:
                                folder = filepath + Path.DirectorySeparatorChar + "SCHWEIF" + Path.DirectorySeparatorChar + "RAW"; break;
                            case DSA2_Archiv.ArchivTyp.SPEECH:
                                folder = filepath + Path.DirectorySeparatorChar + "SCHWEIF" + Path.DirectorySeparatorChar + "SPEECH"; break;
                            case DSA2_Archiv.ArchivTyp.STAR:
                                folder = filepath + Path.DirectorySeparatorChar + "SCHWEIF" + Path.DirectorySeparatorChar + "STAR"; break;

                            case DSA2_Archiv.ArchivTyp.CD_FX:
                                folder = filepath + Path.DirectorySeparatorChar + "CD" + Path.DirectorySeparatorChar + "FX"; break;
                            case DSA2_Archiv.ArchivTyp.CD_RAW:
                                folder = filepath + Path.DirectorySeparatorChar + "CD" + Path.DirectorySeparatorChar + "RAW"; break;
                            case DSA2_Archiv.ArchivTyp.CD_SPEECH:
                                folder = filepath + Path.DirectorySeparatorChar + "CD" + Path.DirectorySeparatorChar + "SPEECH"; break;
                            case DSA2_Archiv.ArchivTyp.CD_SPEECHCD:
                                folder = filepath + Path.DirectorySeparatorChar + "CD" + Path.DirectorySeparatorChar + "SPEECHCD"; break;
                            case DSA2_Archiv.ArchivTyp.CD_STAR:
                                folder = filepath + Path.DirectorySeparatorChar + "CD" + Path.DirectorySeparatorChar + "STAR"; break;
                            case DSA2_Archiv.ArchivTyp.CD_STARCD:
                                folder = filepath + Path.DirectorySeparatorChar + "CD" + Path.DirectorySeparatorChar + "STARCD"; break;
                        }
                        if (folder == null)
                        {
                            MessageBox.Show("Fehler beim exportieren des Archives " + Archiv.Typ.ToString());
                            continue;
                        }
                        
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        for (int i = 0; i < Archiv.entries.Count; i++)
                        {
                            string typ = getFileTyp(Archiv.entries[i].Key);

                            if (!Directory.Exists(folder + Path.DirectorySeparatorChar + typ))
                                Directory.CreateDirectory(folder + Path.DirectorySeparatorChar + typ);

                            int end;
                            if (i >= (Archiv.entries.Count - 1))
                                end = Archiv.data.Length;
                            else
                                end = Archiv.entries[i + 1].Value;

                            string name;
                            if (i < Archiv.entries.Count && Archiv.entries[i].Key != "")
                                name = Archiv.entries[i].Key;
                            else
                                name = i.ToString();

                            byte[] data = new byte[end - Archiv.entries[i].Value];
                            Array.Copy(Archiv.data, Archiv.entries[i].Value, data, 0, data.Length);

                            if (data.Length > 0)
                                File.WriteAllBytes(folder + Path.DirectorySeparatorChar + typ + Path.DirectorySeparatorChar + name, data);
                        }
                    }

                    break;

                default:
                    MessageBox.Show("Fehler beim Entpacken version unbekannt");
                    break;
            }

            MessageBox.Show("entpacken erfolgreich beendet");
        }
        private string getFileTyp(string filename)
        {
            if (filename.Contains("."))
            {
                int i = filename.LastIndexOf('.');
                return filename.Substring(i + 1);
            }

            return "unbekannt";
        }

        public void exportPictures(string filepath)
        {
            CDebugger.addDebugLine("Bilder werden exportiert...");
            foreach (KeyValuePair<string, List<Image>> pair in this.bilder.itsImages)
            {
                string name = getFileFront(pair.Key);
                string path = filepath + Path.DirectorySeparatorChar + "Bilder" + Path.DirectorySeparatorChar + name;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (int i = 0; i < pair.Value.Count; i++)
                    pair.Value[i].Save(path + Path.DirectorySeparatorChar + i.ToString() + ".png");
            }
            foreach (KeyValuePair<string, List<List<Image>>> pair_name in this.bilder.itsAnimations)
            {
                string name = getFileFront(pair_name.Key);
                int counter = 0;

                foreach (List<Image> Images in pair_name.Value)
                {
                    string path = filepath + Path.DirectorySeparatorChar + "Animationen" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + counter;

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    for (int i = 0; i < Images.Count; i++)
                        Images[i].Save(path + Path.DirectorySeparatorChar + i.ToString() + ".png");

                    counter++;
                }
            }
            CDebugger.addDebugLine("Bilder wurden erfolgreich exportiert");
        }
        private string getFileFront(string filename)
        {
            if (filename.Contains("."))
            {
                int i = filename.IndexOf('.');
                return filename.Substring(0, i);
            }

            return filename;
        }


        private class DSA2_Archiv
        {
            public enum ArchivTyp { Unknown, STAR, FX, RAW, SPEECH, CD_STAR, CD_STARCD, CD_FX, CD_RAW, CD_SPEECH, CD_SPEECHCD }
            
            public ArchivTyp Typ = ArchivTyp.Unknown;
            public byte[] data = null;
            public List<KeyValuePair<string, Int32>> entries = new List<KeyValuePair<string, Int32>>();

            public DSA2_Archiv(ArchivTyp typ)
            {
                this.Typ = typ;
            }
        }
    }
}
