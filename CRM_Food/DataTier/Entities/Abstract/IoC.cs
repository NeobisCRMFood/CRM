using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Abstract
{
    public static class IoC
    {
        public static void IoCCommonDataLibraryRegister(this IServiceCollection service)
        {
            service.AddTransient<IEFDbContextRepository, EFDbContextRepository>();
        }
    }
}
