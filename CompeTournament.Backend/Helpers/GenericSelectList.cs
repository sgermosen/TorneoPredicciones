namespace CompeTournament.Backend.Helpers
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GenericSelectList
    {
        public IEnumerable<SelectListItem> CreateSelectList<T>(IList<T> entities, Func<T, object> funcToGetValue, Func<T, object> funcToGetText)
        {
            return entities
                .Select(x => new SelectListItem
                {
                    Value = funcToGetValue(x).ToString(),
                    Text = funcToGetText(x).ToString()
                });
        }
    }
}
