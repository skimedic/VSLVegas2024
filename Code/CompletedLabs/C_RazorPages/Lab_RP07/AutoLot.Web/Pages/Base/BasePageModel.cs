﻿// Copyright Information
// ==================================
// AutoLot8 - AutoLot.Web - BasePageModel.cs
// All samples copyright Philip Japikse
// http://www.skimedic.com 2024/2/4
// ==================================

namespace AutoLot.Web.Pages.Base;

public abstract class BasePageModel<TEntity, TPageModel>(
    IAppLogging<TPageModel> appLoggingInstance,
    IBaseRepo<TEntity> baseRepoInstance,
    string pageTitle) : PageModel where TEntity : BaseEntity, new()
{
    protected readonly IAppLogging<TPageModel> AppLoggingInstance = appLoggingInstance;
    protected readonly IBaseRepo<TEntity> BaseRepoInstance = baseRepoInstance;

    [ViewData]
    public string Title { get; init; } = pageTitle;

    [BindProperty]
    public TEntity Entity { get; set; }
    public SelectList LookupValues { get; set; }
    public string Error { get; set; }

    protected virtual void GetLookupValues()
    {
        LookupValues = null;
    }

    protected virtual void GetOne(int? id)
    {
        if (!id.HasValue)
        {
            Error = "Invalid request";
            Entity = null;
            return;
        }
        Entity = BaseRepoInstance.Find(id.Value);
        if (Entity == null)
        {
            Error = "Not found";
            return;
        }
        Error = string.Empty;
    }

    protected virtual IActionResult SaveOne(
        Func<TEntity,bool,int> persistenceFunction)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
            persistenceFunction(Entity, true);
            return RedirectToPage("./Details", new { id = Entity.Id });
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            ModelState.AddModelError(string.Empty, ex.Message);
            AppLoggingInstance.LogAppError(ex, "An error occurred");
            return Page();
        }
    }
    protected virtual IActionResult SaveWithLookup(Func<TEntity,bool,int> persistenceFunction)
    {
        if (!ModelState.IsValid)
        {
            GetLookupValues();
            return Page();
        }
        try
        {
            persistenceFunction(Entity, true);
            return RedirectToPage("./Details", new { id = Entity.Id });
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            ModelState.AddModelError(string.Empty, ex.Message);
            GetLookupValues();
            AppLoggingInstance.LogAppError(ex, "An error occurred");
            return Page();
        }
    }

    protected virtual IActionResult DeleteOne(int id)
    {
        try
        {
            //throw new Exception("Test");
            BaseRepoInstance.Delete(Entity);
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.Clear();
            Entity = BaseRepoInstance.Find(id);
            Error = ex.Message;
            AppLoggingInstance.LogAppError(ex, "An error occurred");
            return Page();
        }
    }

}
