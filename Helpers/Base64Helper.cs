namespace Base64Tool
{
    using log4net;
    using ShellProgressBar;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;

    public class Base64Helper
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int EncodeBufferSize = 1024 * 3;
        private readonly int DecodeBufferSize = 1024 * 4;
        private readonly ProgressBarOptions options = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true,
            EnableTaskBarProgress = false,
            DisplayTimeInRealTime = false
        };
        private bool displayProgress;

        public Base64Helper(bool displayProgress)
        {
            this.displayProgress = displayProgress;
        }

        public bool EncodeToBase64(string inputFile, string outputFilePath, out long readBytes, out long writtenBytes)
        {
            writtenBytes = 0;
            readBytes = 0;
            try
            {
                readBytes = new FileInfo(inputFile).Length;
                log.Info($"Encoding {readBytes} bytes");
#pragma warning disable CS0642 // Possible mistaken empty statement
                using (File.Create(outputFilePath)) ;
#pragma warning restore CS0642 // Possible mistaken empty statement
            }
            catch
            {
                log.FatalFormat($@"Cannot create output file ""${outputFilePath}"".");
                return false;
            }

            if (readBytes == 0)
            {
                log.Error("Input file is empty. Will not encode.");
                return false;
            }
            using (Stream inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                int offset = 0;
                long lengthToUse = (readBytes < EncodeBufferSize) ? readBytes : EncodeBufferSize;
                byte[] buffer = new byte[lengthToUse];
                int bytesRead = 0;

                using (FileStream fs = new FileStream(outputFilePath, FileMode.Create))
                {
                    string message = String.Empty;
                    ProgressBar progressBar = null;
                    int currentBlock = 1;

                    if (displayProgress)
                    {
                        int blocks = Convert.ToInt32(Decimal.Ceiling(Decimal.Divide(readBytes, lengthToUse)));
                        currentBlock = 1;
                        progressBar = new ProgressBar(blocks, message, options);

                    }
                    do
                    {
                        bytesRead = inputStream.Read(buffer, offset, buffer.Length);

                        if (bytesRead > 0)
                        {
                            if (bytesRead != buffer.Length)
                            {
                                // Just avoiding unnecessary load on GC by allocation to buffer. this would be the last loop.
                                byte[] tempBuffer = new byte[bytesRead];
                                Buffer.BlockCopy(buffer, 0, tempBuffer, 0, bytesRead);
                                buffer = tempBuffer;
                            }
                            string base64String = Convert.ToBase64String(buffer, 0, bytesRead);
                            byte[] base64ByteArray = Encoding.ASCII.GetBytes(base64String);
                            int base64Length = base64ByteArray.Length;
                            fs.Write(base64ByteArray, 0, base64Length);
                            writtenBytes += base64Length;

                            if (progressBar != null)
                            {
                                message = String.Format($"Encoding Block #{currentBlock}");
                                progressBar.Tick(message);
                                currentBlock++;
                            }
                        }
                    } while (bytesRead > 0);

                    if (progressBar != null)
                    {
                        progressBar.Dispose();
                    }
                }
            }
            return true;
        }
        public bool DecodeFromBase64(string inputFile, string outputFilePath, out long processedBytes, out long readBytes)
        {
            readBytes = 0;
            processedBytes = 0;
            try
            {
                readBytes = new FileInfo(inputFile).Length;
                log.Info($"Decoding {readBytes} bytes");
#pragma warning disable CS0642 // Possible mistaken empty statement
                using (File.Create(outputFilePath)) ;
#pragma warning restore CS0642 // Possible mistaken empty statement
            }
            catch
            {
                log.FatalFormat($@"Cannot create output file ""${outputFilePath}"".");
                return false;
            }

            if (readBytes == 0)
            {
                log.Error("Input file is empty. Will not encode.");
                return false;
            }

            using (Stream base64Stream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                int readOffset = 0;
                long lengthToUse = (readBytes < DecodeBufferSize) ? readBytes : DecodeBufferSize;
                byte[] buffer = new byte[lengthToUse];
                int bytesRead = 0;

                using (FileStream fs = new FileStream(outputFilePath, FileMode.Create))
                {
                    int currentBlock = 1;
                    string message = String.Empty;
                    ProgressBar progressBar = null;
                    if (displayProgress)
                    {
                        int blocks = Convert.ToInt32(Decimal.Ceiling(Decimal.Divide(readBytes, lengthToUse)));
                        progressBar = new ProgressBar(blocks, message, options);
                    }

                    do
                    {
                        bytesRead = base64Stream.Read(buffer, readOffset, buffer.Length);

                        if (bytesRead > 0)
                        {
                            if (bytesRead != buffer.Length)
                            {
                                // Just avoiding unnecessary load on GC by allocation to buffer. this would be the last loop.
                                byte[] tempBuffer = new byte[bytesRead];
                                Buffer.BlockCopy(buffer, 0, tempBuffer, 0, bytesRead);
                                buffer = tempBuffer;
                            }

                            string base64String = System.Text.Encoding.ASCII.GetString(buffer);
                            byte[] converted = Convert.FromBase64String(base64String);
                            fs.Write(converted, 0, converted.Length);
                            processedBytes += converted.Length;

                            if (progressBar != null)
                            {
                                message = String.Format($"Decoding Block #{currentBlock}");
                                progressBar.Tick(message);
                                currentBlock++;
                            }
                        }
                    } while (bytesRead > 0);
                    if (progressBar != null)
                    {
                        progressBar.Dispose();
                    }
                }
            }
            return true;
        }
    }
}
