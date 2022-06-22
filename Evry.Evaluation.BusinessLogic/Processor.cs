using Evry.Evaluation.Interfaces.BusinessLogic;
using Evry.Evaluation.Models;
using Evry.Evaluation.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evry.Evaluation.BusinessLogic
{
    public class Processor : IProcessor
    {
        public IEnumerable<ProcessedPersonResult> ProcessEvents(IEnumerable<Event> events, PeriodClass periodClass, IEnumerable<EventType> eventTypes = null)
        {
            // EVAL Optional: Modify to apply event type multipliers.

            var result = new List<ProcessedPersonResult>();

            var currentStart = DateTime.MinValue;
            var currentEnd = DateTime.MinValue;
            var prevStart = DateTime.MinValue;
            var prevEnd = DateTime.MinValue;
            var nextStart = DateTime.MinValue;
            var nextEnd = DateTime.MinValue;

            // EVAL: Get dates for previous and next periods
            switch (periodClass)
            {
                case PeriodClass.Week:
                    currentStart = DateTime.Now.AddDays(((int)DateTime.Now.DayOfWeek - 1) * -1);
                    // EVAL: Get end of week
                    currentEnd = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek);
                    break;
                case PeriodClass.Month:
                    currentStart = DateTime.Now.AddDays((DateTime.Now.Day - 1) * -1);
                    currentEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, 1).AddDays(-1);
                    break;
                case PeriodClass.Quarter:
                    // EVAL

                    DateTime now = DateTime.Now;
                    int currQuarter = (now.Month - 1) / 3 + 1;
                    currentStart = new DateTime(now.Year, 3 * currQuarter - 2, 1);
                    currentEnd = new DateTime(now.Year, 3 * currQuarter + 1, 1).AddDays(-1);
                    break;
                case PeriodClass.Year:
                    currentStart = new DateTime(DateTime.Now.Year, 1, 1);
                    currentEnd = new DateTime(DateTime.Now.Year + 1, 1, 1).AddDays(-1);
                    break;
            }

            var persons = events.Select(x => x.PersonID).ToList();

            persons.ForEach(person =>
            {
                var personResult = new ProcessedPersonResult();
                // EVAL: Fill personal details
                personResult.PersonID = person;

                var takeCurrent = new List<int>();
                var takePrevious = new List<int>();
                var takeNext = new List<int>();
                Dictionary<PeriodType, double> tmpPeriodTotals = new Dictionary<PeriodType, double>();

                for (var i = 0; i < events.Count(); i++)
                {
                    if (events.ToArray()[i].Time >= currentStart)
                    {
                        if (events.ToArray()[i].Time <= currentEnd)
                            takeCurrent.Add(i);
                    }
                    else if (events.ToArray()[i].Time >= prevStart)
                    {
                        if (events.ToArray()[i].Time <= prevEnd)
                            takePrevious.Add(i);
                    }
                    else if (events.ToArray()[i].Time >= nextStart)
                    {
                        if (events.ToArray()[i].Time <= nextEnd)
                            takeNext.Add(i);
                    }
                }

                var currentTotal = 0d;
                for (var i = 0; i < takeCurrent.Count; i++)
                {
                    for (var k = 0; k < events.Count(); k++)
                    {
                        if (k == takeCurrent[i])
                        {
                            if (eventTypes != null)
                            {
                                if (eventTypes.Select(x => x.ID).Contains(events.ToArray()[k].TypeID))
                                {
                                    currentTotal += events.ToArray()[k].Amount;
                                }
                            }
                            else
                            {
                                currentTotal += events.ToArray()[k].Amount;
                            }
                        }
                    }
                }

                tmpPeriodTotals.Add(PeriodType.Current, currentTotal);

                var prevTotal = 0d;
                for (var i = 0; i < takePrevious.Count; i++)
                {
                    for (var k = 0; k < events.Count(); k++)
                    {
                        if (k == takePrevious[i])
                        {
                            if (eventTypes != null)
                            {
                                if (eventTypes.Select(x => x.ID).Contains(events.ToArray()[k].TypeID))
                                {
                                    prevTotal += events.ToArray()[k].Amount;
                                }
                            }
                            else
                            {
                                prevTotal += events.ToArray()[k].Amount;
                            }
                        }
                    }
                }
                tmpPeriodTotals.Add(PeriodType.Previous, prevTotal);

                var nextTotal = 0d;
                for (var i = 0; i < takeNext.Count; i++)
                {
                    for (var k = 0; k < events.Count(); k++)
                    {
                        if (k == takeNext[i])
                        {
                            if (eventTypes != null)
                            {
                                if (eventTypes.Select(x => x.ID).Contains(events.ToArray()[k].TypeID))
                                {
                                    nextTotal += events.ToArray()[k].Amount;
                                }
                            }
                            else
                            {
                                nextTotal += events.ToArray()[k].Amount;
                            }
                        }
                    }
                }
                tmpPeriodTotals.Add(PeriodType.Next, nextTotal);

                personResult.PeriodTotals = tmpPeriodTotals;
                personResult.GrandTotal = prevTotal +currentTotal +nextTotal ;
                result.Add(personResult);
            });

            return result;
        }
    }
}
