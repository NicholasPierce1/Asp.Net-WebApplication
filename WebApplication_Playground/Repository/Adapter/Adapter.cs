using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Repository.Entities;
using WebApplication_Playground.Repository.Repos;

namespace WebApplication_Playground.Repository.Adapter
{
    public sealed class Adapter
    {

        private readonly StudentRepository _studentRepository;

        public Adapter(StudentRepository studentRepository)
        {
            this._studentRepository = studentRepository;
        }

        public IEnumerable<Student> getAllStudents()
        {
            return this._studentRepository.getAllStudents();
        }

    }
}
