using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Media;

#region WAVElib
/// <summary>
/// WAVElib
/// LIMIDE DEV. 2016
/// Библиотека для простой генерации WAV файлов.
/// </summary>
public class WAVElib
{
    #region Вспомогательные методы
    #region Byte[] BytesMerge(Byte[], Byte[])
    private Byte[] BytesMerge(Byte[] a1, Byte[] a2)
    {
        Byte[] res = new Byte[a1.Length + a2.Length];
        for (Int32 i = 0; i < a1.Length; i++)
        {
            res[i] = a1[i];
        }
        for (Int32 i = 0; i < a2.Length; i++)
        {
            res[i + a1.Length] = a2[i];
        }
        return res;
    }
    #endregion
    #region Byte[] BytesMerge(Object[] { Bytes[], Bytes[], ... })
    private Byte[] BytesMerge(Object[] o)
    {
        Int32 fullLength = 0;
        for (Int32 i = 0; i < o.Length; i++)
        {
            fullLength += ((Byte[])o[i]).Length;
        }

        Byte[] res = new Byte[fullLength];

        Int32 posNow = 0;
        for (Int32 i = 0; i < o.Length; i++)
        {
            Byte[] bts = (Byte[])o[i];
            for (Int32 j = 0; j < bts.Length; j++)
            {
                res[posNow] = bts[j];
                posNow++; 
            }
        }

        return res;
    }
    #endregion
    #endregion

    #region public void Load(String path)
    public void Load(Byte [] byt)
    {
           
    }
    #endregion

    #region public get/set SampleRate
    /// <summary>
    /// Принимает значение и ставит частоту дискретизации равной 1/SampleRate.
    /// К примеру, для задания частоты дискретизации равной 44.1 кГц необходимо указать значение 44100.
    /// </summary>
    public UInt32 SampleRate
    {
        get
        {
            return sampleRate;
        }
        set
        {
            sampleRate = value;
            ReCalc();
        }
    }
    #endregion
    #region public get/set BitsPerSample
    /// <summary>
    /// Количество бит выделенных на одну звуковую часть.
    /// Чем больше значение - тем на большее количество частей делится диапазон звучаний.
    /// Доступные значения: 8, 16, 24, 32.
    /// Что соответствует 1, 2, 3 или 4 байтам на звуковую часть.
    /// </summary>
    public UInt16 BitsPerSample
    {
        get
        {
            return bitsPerSample;
        }
        set
        {
            bitsPerSample = value;
            ReCalc();
        }
    }
    #endregion
    #region public get/set Sound
    /// <summary>
    /// Принимает двумерный массив UInt32[X, Y].
    /// Где X - количество звуковых частей, а Y - количество звуковых каналов.
    /// Так, например, для звучания в 1 секунду при частоте дискретизации 44.1 кГц (SampleRate == 44100), необходимо передать в X 44100 значений.
    /// </summary>
    public Int32[,] Sound
    {
        get
        {
            return sound;
        }
        set
        {
            sound = value;
            data = SoundToBytes(sound);
            ReCalc();
        }
    }
    #endregion

    #region public Byte[] GetWAVData()
    /// <summary>
    /// Возвращает байты готового WAV файла в соответствии с заданными значениями.
    /// </summary>
    /// <returns></returns>
    public Byte[] GetWAVData()
    {
        Byte[] fulldata = BytesMerge(new Object[] {
        BitConverter.GetBytes(chunkId),
        BitConverter.GetBytes(chunkSize),
        BitConverter.GetBytes(format),
        BitConverter.GetBytes(subChunk1Id),
        BitConverter.GetBytes(subChunk1Size),
        BitConverter.GetBytes(audioFormat),
        BitConverter.GetBytes(numChannels),
        BitConverter.GetBytes(sampleRate),
        BitConverter.GetBytes(byteRate),
        BitConverter.GetBytes(blockAlign),
        BitConverter.GetBytes(bitsPerSample),
        BitConverter.GetBytes(subChunk2Id),
        BitConverter.GetBytes(subChunk2Size),
        data
        });

        return fulldata;
    }
    #endregion
    #region public Byte[] GetWAVData(UInt32 SampleRate, UInt16 BitsPerSample, Int32[,] Sound)
    /// <summary>
    /// Возвращает байты готового WAV файла в соответствии с заданными значениями. 
    /// Не сохраняет значения в самом объекте, пожтому такие свойства как Duration или Data не будут доступными.
    /// </summary>
    /// <returns></returns>
    public Byte[] GetWAVData(UInt32 SampleRate, UInt16 BitsPerSample, Int32[,] Sound)
    {
        WAVElib wlib = new WAVElib();
        wlib.SampleRate = SampleRate;
        wlib.BitsPerSample = BitsPerSample;
        wlib.Sound = Sound;
        Byte[] res = wlib.GetWAVData();
        wlib.Dispose();
        return res;    
    }
    #endregion

    #region public get Duration
    /// <summary>
    /// Возвращает количество миллисекунд в течении которых будет звучать аудио при параметрах заданных в данный момент. 
    /// </summary>
    public UInt64 Duration
    {
        get
        {
            return (UInt64)(data.Length / blockAlign / (Double)sampleRate * 1000f);    
        }
    }
    #endregion
    #region public get Data 
    /// <summary>
    /// Возвращает одномерный массив чистых звуковых данные файла.
    /// </summary>
    public Byte[] Data
    {
        get
        {
            return data;
        }
    }
    #endregion

    #region public Dispose()
    /// <summary>
    /// Сообщает сборщику мусора о необходимости принудительной очистки из памяти.
    /// </summary>
    public void Dispose()
    {
        GC.Collect();
    }
    #endregion
    #region public GetTestWAVData()
    /// <summary>
    /// Вернёт байты готового тестового сетерео файла длинной в 1 секунду, с частотой дискретизации 44.1 кГц и 32 битным значением звука который вы можете воспроизвести.
    /// Если не вылетит в ошибку - библиотека работает :)
    /// </summary>
    /// <returns></returns>
    public Byte[] GetTestWAVData()
    {
        Int32[,] sound = new Int32[44100, 2];
        Random ran = new Random();

        for (Int32 i = 0; i < sound.GetLength(0); i++)
        {
            sound[i, 0] = (Int32)(Math.Sin(i * 0.1f * (i / 1000f)) * (Int32.MaxValue * 0.05f));
        }

        for (Int32 i = 0; i < sound.GetLength(0); i++)
        {
            sound[i, 1] = (Int32)(Math.Sin(i * 0.1f * Math.Abs(Math.Cos(i / 10000f))) * (Int32.MaxValue * 1f));
        }

        WAVElib wlib = new WAVElib();
        wlib.SampleRate = 44100;
        wlib.BitsPerSample = 32;
        wlib.Sound = sound;

        Byte[] res = wlib.GetWAVData();
        wlib.Dispose();

        return res;
    }
    #endregion

    #region Byte[] SoundToBytes(Int32[,] sound)
    /// <summary>
    /// Переводит двумерный массив sound в массив байт
    /// </summary>
    /// <param name="sound"></param>
    /// <returns></returns>
    private Byte[] SoundToBytes(Int32[,] sound)
    {
        Byte[] res = new Byte[(bitsPerSample / 8) * sound.GetLength(0) * sound.GetLength(1)];
        Int32 byteNow = 0;
        for (Int32 x = 0; x < sound.GetLength(0); x++)
        {
            for (Int32 y = 0; y < sound.GetLength(1); y++)
            {
                Int32 spart = sound[x, y];
                UInt32 uspart = (UInt32)(spart);
                Byte[] bts = UInt32ToBPSBytes(uspart);
                for (Byte i = 0; i < bts.Length; i++)
                {
                    res[byteNow] = bts[i];
                    byteNow++;
                }
            }
        }
        return res;
    }
    #endregion
    #region ReCalc()
    /// <summary>
    /// Производит перерасчёт всех вычисляемых значений, таких как длинна дорожки, количество байт на сэмпл и прочее.
    /// </summary>
    private void ReCalc()
    {
        numChannels = (UInt16)sound.GetLength(1);
        byteRate =(UInt32)(sampleRate * numChannels * (bitsPerSample / 8f));
        blockAlign = (UInt16)(numChannels * (bitsPerSample / 8f));
        chunkSize = (UInt32)(data.LongLength + 36);
        subChunk2Size = (UInt32)(data.LongLength);
    }
    #endregion
    #region Byte[] UInt32ToBPSBytes(UInt32 value)
    /// <summary>
    /// Конвертирует значение полученное в UInt32 в соответствующеие BitsPerSample байты
    /// </summary>
    private Byte[] UInt32ToBPSBytes(UInt32 value)
    {
        if (bitsPerSample == 32)
        {
            return BitConverter.GetBytes(value);
        }
        else
        {
            Double d = (value / (Double)UInt32.MaxValue) * (Math.Pow(255, bitsPerSample / 8f));
            Byte[] bts = BitConverter.GetBytes((UInt32)(d));
            Byte[] res = new Byte[(UInt32)(bitsPerSample / 8f)];
            for (byte i = 0; i < res.Length; i++)
            {
                res[i] = bts[i];
            }
            return res;
        }
    }
    #endregion

    #region Описание фрагментов данных
    // МЕНЯЕТСЯ
    // numChannels <-- задаётся вторым измерением массива data
    // sampleRate
    // bitsPerSample
    // data

    // ВЫСЧИТЫВАЕТСЯ АВТОМАТИЧЕСКИ
    // chunkSize
    // byteRate
    // blockAlign
    // subChunk2Size

    // НЕ МЕНЯЕТСЯ
    // chunkId
    // format
    // subChunk1Id
    // subChunk1Size
    // audioFormat
    // subChunk2Id

    // 44 байта - общая длинна "головы" файла
    // Заголовок файла. 4 байта "RIFF" ASCII
    private UInt32 chunkId = 1179011410; // --- НЕ МЕНЯЕТСЯ
    // Длинна всего файла минус 8 (в байтах) 36 + data.length (если представить что data = 44100 байт при стандартных условиях, то:
    private UInt32 chunkSize = 44136; // --- ВЫСЧИТЫВАЕТСЯ АВТОМАТИЧЕСКИ
    // Формат файла. 4 байта "WAVE" ASCII 
    private UInt32 format = 1163280727; // --- НЕ МЕНЯЕТСЯ
    // Подзаголовок файла. 4 байта "fmt " ASCII
    private UInt32 subChunk1Id = 544501094; // --- НЕ МЕНЯЕТСЯ
    // Размер подчанка. Т.к. не будем использовать сжатие, то значение равно 16. 4 байта                                                     
    private UInt32 subChunk1Size = 16; // --- НЕ МЕНЯЕТСЯ
    // Формат сжатия файла. PCM = 1 (без сжатия) 2 байта
    private UInt16 audioFormat = 1; // --- НЕ МЕНЯЕТСЯ
    // Количество аудио каналов. По умолчанию = 1 (Моно) 2 байта
    private UInt16 numChannels = 1; // --- МЕНЯЕТСЯ
    // Частота дискретизации (по умолчанию 44100, т.е. один звуковой интервал равен 1/44100 секунды, или 44.1 мгц) 4 байта
    private UInt32 sampleRate = 44100; // --- МЕНЯЕТСЯ
    // Количество читаемых в секунду байт ( = sampleRate * numChannels * (bitsPerSample / 8) ) 4 байта
    private UInt32 byteRate = 44100; // --- ВЫСЧИТЫВАЕТСЯ АВТОМАТИЧЕСКИ
    // Количество байт для одного сэмпла ( = numChannels * (bitsPerSample / 8) ) 4 байта
    private UInt16 blockAlign = 1; // --- ВЫСЧИТЫВАЕТСЯ АВТОМАТИЧЕСКИ
    // Количество бит в сэмпле (8, 16, 24 или 32. Т.е. 1, 2, 3 или 4 байта) Грубо говоря, сколькими байтами (и соответственно сколько градаций) у звуковой волны. 2 байта
    private UInt16 bitsPerSample = 8; // --- МЕНЯЕТСЯ
    // Обозначает началo блока данных. Слово "data" ASCII 4 байта                                                        
    private UInt32 subChunk2Id = 1635017060; // --- НЕ МЕНЯЕТСЯ
    // Количество байт в data не считая этой строки. 4 байта (будем считать что по умолчанию 44100 байт)
    private UInt32 subChunk2Size = 44100; // --- ВЫСЧИТЫВАЕТСЯ АВТОМАТИЧЕСКИ
    // Данные звука. Если учитывать дэфолтные настройки, то 1 байт равен 1 сэмплу. 44100 байт = 1 секунде звучания. Градация 0-255 значений.
    public Byte[] data = new Byte[0];
    // "Сырые" данные которые будут преобразованы в массив data
    private Int32[,] sound = new Int32[0, 0]; // --- МЕНЯЕТСЯ
    #endregion
}
#endregion
#region WAVElib_ChannelWorker
public class WAVElib_ChannelWorker
{
    #region public Int32[] Volume(Int32[] sound, Double ratio)
    /// <summary>
    /// И зменяет громкость канала в зависимости от указанного коэффициента.
    /// 0 - нет звука. 1 - оригинальная громкость. 1.5 - увелечение в пол раза.
    /// </summary>
    /// <returns></returns>
    public Int32[] Volume(Int32[] channel, Double ratio)
    {
        Int32[] newChannel = new Int32[channel.Length];
        for (Int32 i = 0; i < channel.Length; i++)
        {
            newChannel[i] = (Int32)(channel[i] * ratio);
        }
        return newChannel;
    }
    #endregion
    #region public Int32[] Copy(Int32[] channel, Int32 start, Int32 length)
    /// <summary>
    /// Копирует звуковой канал полностью или на заданом отрезке.
    /// </summary>
    /// <returns></returns>
    public Int32[] Copy(Int32[] channel, Int32 start = 0, Int32 length = -1)
    {
        if (length < 0) { length = channel.Length - start; }
        if (length < 0) { return new Int32[0]; }
        Int32[] copy = new Int32[length];

        for (Int32 i = 0; i < length; i++)
        {
            copy[i] = channel[i + start];
        }
        return copy;
    }
    #endregion
    #region public Int32[] Add(Int32[] channel1, Int32[] channel2)
    /// <summary>
    /// Добавляет один звуковой канал к другому.
    /// </summary>
    /// <returns></returns>
    public Int32[] Add(Int32[] channel1, Int32[] channel2)
    {
        Int32[] channel3 = new Int32[channel1.Length + channel2.Length];

        for (Int32 i = 0; i < channel1.Length; i++)
        {
            channel3[i] = channel1[i];
        }

        for (Int32 i = 0; i < channel2.Length; i++)
        {
            channel3[i + channel1.Length] = channel2[i];
        }

        return channel3;
    }
    #endregion
    #region public Int32[] AddAll(Object[] channels)
    /// <summary>
    /// Склеивает последовательно в один несколько звуковых каналов
    /// </summary>
    /// <returns></returns>
    public Int32[] AddAll(Object[] channels)
    {
        Int32[] newChannel = new Int32[0];
        for (Int32 i = 0; i < channels.Length; i++)
        {
            newChannel = Add(newChannel, (Int32[])channels[i]);
        }
        return newChannel;
    }
    #endregion
    #region public Int32[] Delete(Int32[] channel, Int32 start, Int32 length)
    /// <summary>
    /// Удаляет из звукового канала отрезок.
    /// </summary>
    /// <returns></returns>
    public Int32[] Delete(Int32[] channel, Int32 start = 0, Int32 length = -1)
    {
        if (length < 0) { length = channel.Length - start; }
        Int32[] c1 = Copy(channel, 0, start);
        Int32[] c2 = Copy(channel, length);
        return Add(c1, c2);
    }
    #endregion
    #region public Int32[] Replace(Int32[] channel, Int32[] subchannel, Int32 start)
    /// <summary>
    /// Заменяет кусок в channel другим subchannel начиная с позиции start
    /// </summary>
    /// <returns></returns>
    public Int32[] Replace(Int32[] channel, Int32[] subchannel, Int32 start)
    {
        Int32[] newChannel = new Int32[channel.Length];
        for (Int32 i = 0; i < channel.Length; i++)
        {
            if (i < start || i > start + subchannel.Length - 1)
            {
                newChannel[i] = channel[i];
            }
            else
            {
                newChannel[i] = subchannel[i - start];
            }
        }
        return newChannel;
    }
    #endregion
    #region public Int32[] Insert(Int32[] channel, Int32 subchannel, Int32 start)
    /// <summary>
    /// Вставляет второй канал в первый в позицию start
    /// </summary>
    /// <returns></returns>
    public Int32[] Insert(Int32[] channel, Int32[] subchannel, Int32 start)
    {
        Int32[] c1 = Copy(channel, 0, start);
        Int32[] c2 = Copy(channel, start);
        Int32[] newChannel = Add(c1, subchannel);
        newChannel = Add(newChannel, c2);
        return newChannel;
    }
    #endregion

    #region public Int32[] Generate_Sinus(Double freq, Double vol, Int32 count)
    /// <summary>
    /// Генерирует звуковую волну длинной в count звуковых единиц с заданым парметром частоты (0-1), громкости.
    /// </summary>
    /// <returns></returns>
    public Int32[] Generate_Sinus(Double freq, Double vol, Int32 count)
    {
        Int32[] channel = new Int32[count];
        for (Int32 i = 0; i < count; i++)
        {
            channel[i] = (Int32)(Math.Sin(i * freq) * (Int32.MaxValue * vol));
        }
        return channel;
    }
    #endregion
    #region public Int32[] Generate_Noise(Double vol, Int32 count)
    /// <summary>
    /// Генерирует шум заданной громкости и длинны
    /// </summary>
    /// <returns></returns>
    public Int32[] Generate_Noise(Double vol, Int32 count)
    {
        Int32[] channel = new Int32[count];
        Random ran = new Random();
        for (Int32 i = 0; i < count; i++)
        {
            channel[i] = (Int32)(ran.NextDouble() * vol * Int32.MaxValue * ((ran.Next(0, 2) == 0) ? -1 : 1));
        }
        return channel;
    }
    #endregion

    #region public Int32[] Process_VolumeDownLinear(Int32[] channel, Int32 start = 0, Int32 length = -1)
    /// <summary>
    /// Линейно уменьшает громкость звука.
    /// </summary>
    /// <returns></returns>
    public Int32[] Process_VolumeDownLinear(Int32[] channel, Int32 start = 0, Int32 length = -1)
    {
        if (length < 0) { length = channel.Length - start; }

        for (Int32 i = start; i < start + length; i++)
        {
            channel[i] = (Int32)(channel[i] * (1 - ((1f / length) * (i - start))));
        }

        return channel;
    }
    #endregion
    #region public Int32[] Process_VolumeUpLinear(Int32[] channel, Int32 start = 0, Int32 length = -1)
    /// <summary>
    /// Линейно увеличивает громкость звука.
    /// </summary>
    /// <returns></returns>
    public Int32[] Process_VolumeUpLinear(Int32[] channel, Int32 start = 0, Int32 length = -1)
    {
        if (length < 0) { length = channel.Length - start; }

        for (Int32 i = start; i < start + length; i++)
        {
            channel[i] = (Int32)(channel[i] * ((1f / length) * (i - start)));
        }

        return channel;
    }
    #endregion
    #region public Int32[] Process_MergeChannel(Object[] channels)
    /// <summary>
    /// Объединяет несколько каналов в один вычисляя среднее значение на каждом промежутке.
    /// </summary>
    /// <returns></returns>
    public Int32[] Process_MergeChannel(Object[] channels)
    {
        Int32[] channel3 = new Int32[((Int32[])channels[0]).Length];
        for (Int32 i = 0; i < ((Int32[])channels[0]).Length; i++)
        {
            for (Int32 ch = 0; ch < channels.Length; ch++)
            {
                channel3[i] += ((Int32[])channels[ch])[i];
            }
            channel3[i] = (Int32)((Double)channel3[i] / (Double)channels.Length);
        }
        return channel3;
    }
    #endregion
    #region public Int32[] Process_CountTo(Int32[] channel, Int32 count)
    /// <summary>
    /// Приводит канал к определённой длинне путём растягивания или сжатия
    /// </summary>
    /// <returns></returns>
    public Int32[] Process_CountTo(Int32[] channel, Int32 count)
    {
        Int32[] newChannel = new Int32[count];
        Double coi = (Double)(channel.Length) / count;

        for (Int32 i = 0; i < count; i++)
        {
            newChannel[i] = channel[(Int32)(i * coi)];
        }

        return newChannel;
    }
    #endregion
}
#endregion
#region WAVElib_SoundWorker
public class WAVElib_SoundWorker
{
    #region public Int32[] GetChannel(Int32[,] sound, Int32 num)
    /// <summary>
    /// Возвращает определённый канал из массива - аудо
    /// </summary>
    /// <returns></returns> 
    public Int32[] GetChannel(Int32[,] sound, Int32 num)
    {
        Int32[] channel = new Int32[sound.GetLength(0)];
        for (Int32 i = 0; i < channel.Length; i++)
        {
            channel[i] = sound[i, num];
        }
        return channel;
    }
    #endregion
    #region public Int32[,] MakeSound(Object[] channels)
    /// <summary>
    /// Принимает массив массивов типа Int32[] - каналов и склеивает их в один.
    /// Длинна всех каналов должна быть одинаковой!
    /// </summary>
    /// <returns></returns>
    public Int32[,] MakeSound(Object[] channels)
    {
        Int32[,] sound = new Int32[((Int32[])channels[0]).Length, channels.Length];
        for (Int32 ch = 0; ch < channels.Length; ch++)
        {
            for (Int32 i = 0; i < sound.GetLength(0); i++)
            {
                sound[i, ch] = ((Int32[])channels[ch])[i];
            }
        }
        return sound;
    }
    #endregion
}
#endregion

namespace WAVE_lib
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Объявление необходимых компонентов
            WAVElib wlib = new WAVElib();
            WAVElib_SoundWorker sowork = new WAVElib_SoundWorker();
            WAVElib_ChannelWorker chwork = new WAVElib_ChannelWorker();
            #endregion

            Int32[] chSin = chwork.Generate_Sinus(0.05f, 0.3f, 44100 * 4);
            Int32[] chN1 = chwork.Generate_Noise(0.5f, 44100 * 4);
            Int32[] chN2 = chwork.Generate_Noise(0.5f, 44100 * 4);

            Int32[] chLeft = chwork.Process_MergeChannel(new Object[] { chSin, chN1, });
            Int32[] chRight = chwork.Process_MergeChannel(new Object[] { chSin, chN2, });

            chLeft = chwork.Process_VolumeUpLinear(chLeft, 0, 44100);
            chLeft = chwork.Process_VolumeDownLinear(chLeft, 44100 * 3, 44100);

            chRight = chwork.Process_VolumeUpLinear(chRight, 0, 44100);
            chRight = chwork.Process_VolumeDownLinear(chRight, 44100 * 3, 44100);

            Int32[,] sound = sowork.MakeSound(new Object[] { chLeft, chRight, });
            
            // Установка значений генерации
            wlib.SampleRate = 44100;
            wlib.BitsPerSample = 32;           

            // Воспроизведение
            wlib.Sound = sound;
            new SoundPlayer(new MemoryStream(wlib.GetWAVData())).Play();

            //File.WriteAllBytes("w.wav", wlib.GetWAVData());

            #region Рисование прогресс-бара
            for (Int32 i = 0; i < 120; i++)
            {
                System.Threading.Thread.Sleep((Int32)(wlib.Duration / 120f));
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(Math.Round(i / 120f * wlib.Duration).ToString() + " / " + (wlib.Duration).ToString() + "ms " + Math.Round(i * (10f / 12f)).ToString() + "%");
                Console.SetCursorPosition(i, 1);
                Console.Write("=");   
            }
            Console.WriteLine("");
            Console.WriteLine("END");
            Console.ReadKey();
            #endregion
        }
    }
}
