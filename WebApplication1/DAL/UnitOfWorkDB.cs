using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.DAL {
    public class UnitOfWorkDB : IDisposable {

        //private GenericRepository<Department> departmentRepository;
        //private GenericRepository<Course> courseRepository;
        public NFLdatabase NFLContext;
        public FF FFContext;
        public UnitOfWorkDB() {
            FFContext = new FF();
            NFLContext = new NFLdatabase();
        }
        //public GenericRepository<Department> DepartmentRepository {
        //    get {

        //        if (this.departmentRepository == null) {
        //            this.departmentRepository = new GenericRepository<Department>(context);
        //        }
        //        return departmentRepository;
        //    }
        //}

        //public GenericRepository<Course> CourseRepository {
        //    get {

        //        if (this.courseRepository == null) {
        //            this.courseRepository = new GenericRepository<Course>(context);
        //        }
        //        return courseRepository;
        //    }
        //}
        public void AddRangeNFLTeamToDB(List<NFLTeam> t) {
            if (FFContext.NFLTeam.Count() == 0)
                FFContext.NFLTeam.AddRange(t);
        }
        public void AddNFLTeamToDB(NFLTeam t) {
            FFContext.NFLTeam.Add(t);
        }
        public void AddNFLGameToDB(NFLGame g) {
            FFContext.NFLGame.Add(g);
        }

        public void FFSave() {
            FFContext.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    FFContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


    }
}