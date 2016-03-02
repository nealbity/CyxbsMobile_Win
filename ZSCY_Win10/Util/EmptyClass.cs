﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZSCY_Win10.Data;

namespace ZSCY_Win10.Util
{
    class EmptyClass
    {
        /// <summary>
        /// 要查询的周次
        /// </summary>
        public int Weeknum { get; set; }
        /// <summary>
        /// 所有学号的课程，数据源
        /// </summary>
        public Dictionary<string, List<ClassListLight>> Searchlist { get; set; }

        public EmptyClass(int weeknum, Dictionary<string, List<ClassListLight>> searchlist)
        {

            this.Weeknum=weeknum;
            this.Searchlist = searchlist;
            if (weeknum < 0 && weeknum !=-100)
            {
                this.Weeknum = 11;
            }
        }
        public void getfreetime(ObservableCollection<ClassListLight> weekresult, ObservableCollection<EmptyTable> termresult)
        {
            //所有人的名字
            string[] names = (from n in Searchlist.Keys select n).ToArray();
            //星期，时间段，人名数组
            List<ClassListLight> clist = new List<ClassListLight>();
            if (Weeknum != -100)
            {
                foreach (var key in Searchlist.Keys)
                {
                    //找到该周的所有课程
                    clist.AddRange((from n in Searchlist[key] where n.Week.Contains(Weeknum) select n).ToList());
                }
                //添加都没课的时间
                //day和lesson都没有在clist里出现过就添加一个classlistlight对象
                for (int i = 0; i < 7; i++)//一周
                {
                    for (int j = 0; j < 6; j++)//一天
                    {
                        //查时间有没有在集合里出现过
                        ClassListLight ourfreetime = new ClassListLight() { Hash_day = i, Hash_lesson = j };
                        if (!clist.Contains(ourfreetime, new ClassListLigthCompare()))
                        {
                            ourfreetime.Name = names;
                            clist.Add(ourfreetime);
                        }
                    }
                }
                //clist = clist.OrderBy(x => x.Hash_day).ToList();
                //筛选出该周内所有不在同一时间上课的课    
                var diisclist = from n in clist group n by new { n.Hash_day, n.Hash_lesson } into g where g.Count() < names.Length select g;
                var ll = diisclist.ToList();
                for (int i = 0; i < ll.Count; i++)
                {
                    ClassListLight tobeadded = ll[i].ToList()[0].Clone();
                    if (tobeadded.Name.Length!= names.Length)
                    {
                        tobeadded.Name = names.Except(tobeadded.Name).ToArray();
                    }
                    weekresult.Add(tobeadded);
                }
                //大家都没课的时间
            }
            else
            {
                //todo学期空课表
                //我要怎么做呢，怎么做呢，既然要查学期，那么就按课的时间差吧，从星期一第一节到星期天
                //查每个人，每个时间段有课情况
                //查到所有这个时间段的周：EX :星期一第一二节课，  得到一个周的列表   1,2,3,4,5,6     得到每个人的周列表，2,4,6,8    1,3,5,7 ，找出都没课的周
                //怎么找呢   
                //找出每个人没课的周 7,8,9,10  1,3,4,7,9,10  2,4,5,8,10
                //那么就得到了           张三            历史           王五
                //那么这个类就应该是 周，时间，姓名[]，那几周空[]----周，时间，  键值对（名字，有空的周[]）
                //前面挖了个坑，这里又要一个类，EmptyTable，不兼容啊卧槽。
                int[] allweeks = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
                for (int i = 0; i < 7; i++)//星期
                {
                    for (int j = 0; j < 6; j++)//时段
                    {
                        EmptyTable temp = new EmptyTable { Hash_day = i, Hash_lesson = j };
                        temp.nameweek = new Dictionary<string, int[]>();
                        foreach (var key in Searchlist.Keys)
                        {
                            IEnumerable<int[]> a = from n in Searchlist[key] where n.Hash_day == i && n.Hash_lesson == j select n.Week;
                            //var b=from x in Searchlist group x.Value by x.Key into g 
                            List<int[]> listweek = a.ToList();
                            List<int> onweeks = new List<int>();
                            for (int m = 0; m < listweek.Count; m++)
                            {
                                onweeks.AddRange(listweek[m]);
                            }
                            int[] free = allweeks.Except(onweeks.ToArray()).ToArray();
                            free = free.Distinct().ToArray() ;
                            if (listweek.Count == 0)//这个时候我没课
                            {
                                temp.nameweek.Add(key, allweeks);
                            }
                            else if (free.Length == 18)
                            {
                                //这个时候我一学期都有课
                                continue;
                            }
                            else//这个时候我有课,找出我没课的周
                            {
                                temp.nameweek.Add(key, free);
                            }
                        }
                        termresult.Add(temp);
                    }
                }
            }
        }
        private class ClassListLigthCompare : IEqualityComparer<ClassListLight>
        {
            public bool Equals(ClassListLight x, ClassListLight y)
            {
                if (x.Hash_day == y.Hash_day && x.Hash_lesson == y.Hash_lesson)
                {
                    return true;
                }
                else
                    return false;
            }

            public int GetHashCode(ClassListLight obj)
            {
                return 0;
            }
        }
    }
}
