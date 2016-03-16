using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_activator
{
    class Parcer
    {
        /// <summary>
        /// The method return sub string between specified strings / Метод возвращает подстроку из исходной между заданными сроками
        /// </summary>
        /// <param name="_source">Source string / Исходная строка</param>
        /// <param name="_start">Start string or symbol / Начальная строка или символ</param>
        /// <param name="_end">End string or symbol / Конечная строка или символ</param>
        /// <returns></returns>
        public static string SubString(string _source, string _start, string _end)
        {
            if (_source.Contains(_start))
            {
                int posStart = _source.IndexOf(_start);
                string retStr = _source.Remove(0, posStart + _start.Length);

                if (!retStr.Contains(_end))
                    return retStr;

                int posEnd = retStr.IndexOf(_end);
                retStr = retStr.Remove(posEnd);

                return retStr;
            }

            return null;
        }
    }
}
