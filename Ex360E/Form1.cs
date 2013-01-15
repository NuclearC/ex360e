using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using X360;
using X360.IO;
using X360.STFS;
using StrongNameRemove;

namespace Ex360E
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void installGamePackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (File.Exists(ofd.FileName))
            {
                // Open STFS Package
                STFSPackage package = new STFSPackage(ofd.FileName, null);

                // Unpack Archive
                string titleName = package.Header.Title_Display;
                string path = "Games/" + titleName;
                if (Directory.Exists(path))
                {
                    package.CloseIO();
                    goto CheckXNA;
                }
                Directory.CreateDirectory(path);
                MessageBox.Show("Ex360E will now install the game package, this may take a while\n and may appear to stop responding. Please be patient.");
                package.ExtractPayload(path, true, false);          
                package.CloseIO();
            CheckXNA:
                path += "/Root/";  
                // Check if XNA title
                if (!Directory.Exists(path + "Runtime"))
                {
                    MessageBox.Show("Package not supported\nOnly XBLA games created with XNA Game Studio currently work.");
                    return;
                }

                // Check if supported
                string[] directories = Directory.GetDirectories(path + "Runtime");

                string frameworkVersion = "Unsupported";
                for (int i = 0; i < directories.Length; i++)
                {
                    // Currently only supports XNA 3.1
                    if (directories[i].Contains("v3.1"))
                    {
                        frameworkVersion = "v3.1";
                    }
                }
                if (frameworkVersion == "Unsupported")
                {
                    MessageBox.Show("Sorry, this game uses a currently unsupported version of the XNA Framework.");
                    return;
                }

                // Decrypt XEX Files
                string[] xexFiles = Directory.GetFiles(path, "*.xex", SearchOption.AllDirectories);
                for (int i = 0; i < xexFiles.Length; i++)
                {
                    ProcessStartInfo info = new ProcessStartInfo("xextool.exe", "-b " + xexFiles[i].Replace(".xex", "") + " " + xexFiles[i]);
                    info.CreateNoWindow = true;
                    info.UseShellExecute = false;
                    info.RedirectStandardOutput = true;
                    Process proc = new Process();
                    proc.StartInfo = info;
                    proc.Start();
                    proc.WaitForExit();
                }

                // Trim useless PE Headers, leaving .NET assemblies behind
                string[] dllFiles = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
                string[] exeFiles = Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories);
                
                for (int i = 0; i < dllFiles.Length; i++)
                {
                    string tmpFileName = dllFiles[i] + ".tmp";
                    FileStream inStream = new FileStream(dllFiles[i], FileMode.Open);
                    FileStream outStream;

                    // Set position
                    inStream.Position = 0x30000;

                    // Read Magic Number
                    byte[] magic = new Byte[2];
                    inStream.Read(magic ,0, 2);

                    // Check for MZ Header
                    if (magic[0] == 0x4D && magic[1] == 0x5A)
                    {
                        outStream = new FileStream(tmpFileName, FileMode.Create);
                        // Reset Position
                        inStream.Position = 0x30000;

                        // Copy data to temporary file
                        int bufferSize = (int)inStream.Length - 0x30000;
                        byte[] outData = new byte[bufferSize];
                        inStream.Read(outData, 0, bufferSize);
                        inStream.Close();
                        outStream.Write(outData, 0, bufferSize);
                        outStream.Flush();
                        outStream.Close();
                        File.Delete(dllFiles[i]);
                        File.Move(tmpFileName, dllFiles[i]);
                    }
                }

                for (int i = 0; i < exeFiles.Length; i++)
                {
                    string tmpFileName = dllFiles[i] + ".tmp";
                    FileStream inStream = new FileStream(exeFiles[i], FileMode.Open);
                    FileStream outStream;

                    // Set position
                    inStream.Position = 0x30000;

                    // Read Magic Number
                    byte[] magic = new Byte[2];
                    inStream.Read(magic, 0, 2);

                    // Check for MZ Header
                    if (magic[0] == 0x4D && magic[1] == 0x5A)
                    {
                        outStream = new FileStream(tmpFileName, FileMode.Create);

                        // Reset Position
                        inStream.Position = 0x30000;

                        // Copy data to temporary file
                        int bufferSize = (int)inStream.Length - 0x30000;
                        byte[] outData = new byte[bufferSize];
                        inStream.Read(outData, 0, bufferSize);
                        inStream.Close();
                        outStream.Write(outData, 0, bufferSize);
                        outStream.Flush();
                        outStream.Close();
                        File.Delete(exeFiles[i]);
                        File.Move(tmpFileName, exeFiles[i]);
                    }
                }

                // Patch And Copy Runtime Files
                string[] XNALibs = Directory.GetFiles(path + "Runtime/" + frameworkVersion, "*.dll");
                for (int i = 0; i < XNALibs.Length; i++)
                {
                    // Patch files
                    string patchFile = XNALibs[i].Replace(path, "Patches/") + ".xdelta";
                    string destFile = XNALibs[i].Replace("Runtime/" + frameworkVersion, "");

                    // If patch exists, apply it 
                    if (File.Exists(patchFile))
                    {
                        ProcessStartInfo info = new ProcessStartInfo("xdelta", " -d -f -s " + XNALibs[i] + " " + patchFile + " " + destFile);
                        info.CreateNoWindow = true;
                        info.UseShellExecute = false;
                        info.RedirectStandardOutput = true;
                        Process proc = new Process();
                        proc.StartInfo = info;
                        proc.Start();
                        proc.WaitForExit();
                    }

                    // Copy un-patched files
                    if (!File.Exists(destFile))
                    {
                        File.Move(XNALibs[i], XNALibs[i].Replace("Runtime/" + frameworkVersion, ""));
                    }
                    
                    // Patch to remove assembly verification
                    DisableStrongNameSignatures(destFile);
                }
               
                // Patch Game Files
                string[] GameFiles = Directory.GetFiles(path, "*.*");
                for (int i = 0; i < GameFiles.Length; i++)
                {
                    // Patch files
                    string patchFile = GameFiles[i].Replace(path, "Patches/Games/" + titleName + "/") + ".xdelta";
                    string destFile = GameFiles[i] + ".patched";

                    // If patch exists, apply it 
                    if (File.Exists(patchFile))
                    {
                        ProcessStartInfo info = new ProcessStartInfo("xdelta", " -d -f -s " + GameFiles[i] + " " + patchFile + " " + destFile);
                        info.CreateNoWindow = true;
                        info.UseShellExecute = false;
                        info.RedirectStandardOutput = true;
                        Process proc = new Process();
                        proc.StartInfo = info;
                        proc.Start();
                        proc.WaitForExit();
                        File.Delete(GameFiles[i]);
                        File.Move(destFile, GameFiles[i]);
                    }

                    DisableStrongNameSignatures(GameFiles[i]);
        
                }
               

                // Copy Xbox 360 Emulation libraries
                string[] X360Libs = Directory.GetFiles("XboxLibs", "*.dll");
                for (int i = 0; i < X360Libs.Length; i++)
                {
                    File.Copy(X360Libs[i], X360Libs[i].Replace("XboxLibs", path), true);
                }

                // Ask to create desktop shortcut to game

                // Ask if game should be launched now
            }
        }

        public void DisableStrongNameSignatures(string filename)
        {
            uint cliHeaderFlag = 0;
            long cliHeaderFlagOffset = 0;
            long strongNameSignatureOffset = 0;
            long publicKeyIndexOffset = 0;
            long publicKeyOffset = 0;
            uint assemblyFlag = 0;
            long assemblyFlagOffset = 0;
            string compiledRuntimeVersion = String.Empty;
            ArrayList assemblyReferences = new ArrayList();
            int blobIndexSize = 0;
            string peKind = String.Empty;

            Utility.GetAssemblyData(filename, ref cliHeaderFlag, ref cliHeaderFlagOffset, ref strongNameSignatureOffset, ref publicKeyIndexOffset, ref publicKeyOffset, ref assemblyFlag, ref assemblyFlagOffset, ref compiledRuntimeVersion, ref assemblyReferences, ref blobIndexSize, ref peKind);
            // Check if strong signed, if so, remove signing
            if (Utility.IsAssemblyStrongSigned(cliHeaderFlag))
            {
                Utility.PatchAssemblyStrongSigning(filename, cliHeaderFlag, cliHeaderFlagOffset, strongNameSignatureOffset, publicKeyIndexOffset, assemblyFlag, assemblyFlagOffset, blobIndexSize);
            }
            // Patch references
            foreach (AssemblyReference reference in assemblyReferences)
            {
                // Don't patch mscorlib references!
                if (!reference.ReferenceName.Contains("mscorlib"))
                {
                    if (reference.PublicKeyOrToken.Length > 0)
                    {
                        Utility.PatchReference(filename, reference.ReferenceOffset, blobIndexSize);
                    }
                }
            }
        }
    }
}
