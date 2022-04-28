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

        public IEnumerable<Student> getStudentsByGender(Student.Gender gender)
        {
            return this._studentRepository.getStudentsByGender(gender);
        }

        public IEnumerable<Student> getStudentsByGenderAndNameLength(Student.Gender gender, int length)
        {
            return this._studentRepository.getStudentsByGenderAndNameLength(gender, length);
        }

        public int updateStudentWithLowTestScore(int threshold, bool increase, bool rollback)
        {
            return this._studentRepository.updateStudentWithLowTestScore(threshold, increase, rollback);
        }

        public string simulateBatchSave(string failAt)
        {
            return this._studentRepository.simulateBatchSave(failAt);
        }

    }
}
