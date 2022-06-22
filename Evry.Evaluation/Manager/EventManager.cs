using Evry.Evaluation.BusinessLogic;
using Evry.Evaluation.Models;
using Evry.Evaluation.Models.Enums;
using Evry.Evaluation.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Evry.Evaluation.Manager
{
    public class EventManager
    {
        public List<EventViewModel> GetEventList()
        {
            var result = new List<EventViewModel>();
            using (var repo = new DataRepository())
            {
                // EVAL: Get data from repository and fill view models

                var events = repo.GetEvents().ToList();
                var proc = new Processor();
                var processedpersonresult = proc.ProcessEvents(events, PeriodClass.Quarter);

                foreach (var processedperson in processedpersonresult)
                {
                    var eventviewmodel = new EventViewModel();
                    eventviewmodel.ID = new Guid();
                    eventviewmodel.TypeName = PeriodClass.Quarter.ToString();
                    var person = repo.GetPeople().Where(p => p.ID == processedperson?.PersonID).FirstOrDefault();
                    eventviewmodel.PersonName = person.Firstname + " " + person.Lastname;
                    eventviewmodel.Region = repo.GetRegionById(person.RegionID).Name.ToString();
                    eventviewmodel.Time = person.Starts;
                    var test = processedperson.PeriodClass.GetType().FullName;
                    if (processedperson.PeriodTotals[PeriodType.Current] >0)
                    {
                        eventviewmodel.Amount = processedperson.PeriodTotals[PeriodType.Current];
                    }              
                    eventviewmodel.Sum = processedperson.GrandTotal;
                    result.Add(eventviewmodel);
                }
            }
            return result;
        }
    }
}