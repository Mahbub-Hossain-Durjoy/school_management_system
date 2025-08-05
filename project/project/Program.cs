
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using project;
using System.Runtime.ConstrainedExecution;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

using var context = new SchoolDbContext();

#region SeedUsers
static void SeedUsers(SchoolDbContext context)
{

    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User { Username = "admin_delwer", Password = "1234" }
        );
        context.SaveChanges();
        Console.WriteLine("Dafault Admin created.");
    }
}

SeedUsers(context);

#endregion  

#region Login

while (true)
{
    
    var loginResult = Login(context);

    if (loginResult == null)
    {
        Console.WriteLine("Login Failed");
        continue;
    }

    string role = loginResult.Value.role;
    string username = loginResult.Value.username;

    if (role == "admin")
    {
        Console.Write("Welcome Admin, ");
        AdminMenu(context);
    }
    else if (role == "teacher")
    {
        Console.Write($"\nWelcome {username}, ");
        TeacherMenu(context, username);
    }
    else
    {
        Console.WriteLine("Login Failed");
    }

    Console.WriteLine("You have been logged out");
}
static (string role, string username)? Login(SchoolDbContext context)
{
    Console.WriteLine("Please login:");

    Console.Write("Username: ");
    string username = Console.ReadLine();

    Console.Write("Password: ");
    string password = Console.ReadLine();

    var user = context.Users
        .FirstOrDefault(x => x.Username == username && x.Password == password);

    if (user == null)
    {
        Console.WriteLine("Invalid credentials");
        return null;
    }

    if (username.StartsWith("admin_"))
    {
        return ("admin", username);
    }
    else if (username.StartsWith("teacher_"))
    {

        string teacherName = username.Replace("teacher_", "");

        var existingTeacher = context.Teachers.FirstOrDefault(t => t.UserId == user.Id);

        if (existingTeacher == null)
        {
            Console.WriteLine("Teacher not found.");
            return null;
        }

        return ("teacher", username);
    }

    return null;
}
#endregion

#region AdminMenu
static void AdminMenu(SchoolDbContext context)
{
    while (true)
    {
        
        Console.Write("Please Select an option: ");
        Console.WriteLine("\n1) Create class");
        Console.WriteLine("2) Create subject");
        Console.WriteLine("3) Create teacher");
        Console.WriteLine("4) View classes");
        Console.WriteLine("5) View subjects");
        Console.WriteLine("6) View teachers");
        Console.WriteLine("7) Class-Subject-Teacher Mapping");
        Console.WriteLine("0) Logout");

        Console.Write("Enter your choice: ");
        var input = Console.ReadLine();
        
        switch (input)
        {
            case "1":
                CreateClass(context);
                ClassSubMenu(context);
                break;
            case "2":
                CreateSubject(context);
                SubjectSubMenu(context);
                break;
            case "3":
                CreateTeacher(context);
                TeacherSubMenu(context);
                break;
            case "4":
                ListClasses(context);
                ClassSubMenu(context);
                break;
            case "5":
                ListSubjects(context);
                SubjectSubMenu(context);
                break;
            case "6":
                ListTeachers(context);
                TeacherSubMenu(context);
                break;
            case "7":
                ViewAssignments(context);
                break;

                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option");
                break;
        }

    }
}
#endregion

#region TeacherMenu
static void TeacherMenu(SchoolDbContext context, string username)
{
    var teacher = context.Teachers
        .Include(t => t.User)
        .FirstOrDefault(t => t.User.Username == username);

    if (teacher == null)
    {
        Console.WriteLine("Teacher not found.");
        return;
    }

   

    while (true)
    {
        Console.Write("Please select an opniton: \n");
        Console.WriteLine("\n1) View grades");
        Console.WriteLine("2) Insert grades");
        Console.WriteLine("3) View grades by class");
        Console.WriteLine("4) Add Students");
        Console.WriteLine("0) Logout");

        Console.Write("Enter your choice: ");
        string input = Console.ReadLine();

        switch (input)
        {
            case "1":
                ViewGrades(context, teacher.Id);
                break;
            case "2":
                InsertGrades(context, teacher.Id);
                break;
            case "3":
                ShowAllGradesByClass(context);
                break;
            case "4":
                AddStudent(context);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }
}
static void CreateTeacher(SchoolDbContext context)
{
    Console.WriteLine("Please provide following information to create a new teacher: ");
    Console.Write("Teacher Name: ");
    string name = Console.ReadLine();

    Console.Write("Teacher username: ");
    string username = Console.ReadLine();

    Console.Write("Teacher password: ");
    string password = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(name) ||
        string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("All fields are required. Teacher creation failed.");
        return;
    }

    if (!username.StartsWith("teacher_"))
    {
         username = "teacher_" + username.ToLower();
    }
        
    if (context.Users.Any(u => u.Username == username))
    {
        Console.WriteLine("A user with this name already exists.");
        return;
    }

    var user = new User { Username  = username, Password = password };

    context.Users.Add(user);
    context.SaveChanges();

    var teacher = new Teacher { Name = name, UserId = user.Id };

    context.Teachers.Add(teacher);
    context.SaveChanges();
    Console.WriteLine($"Teacher '{name}' created successfully with username '{username}'.");
   
}
static void ListTeachers(SchoolDbContext context)
{
    var teachers = context.Teachers.Include(t => t.User).ToList();

    if (!teachers.Any())
    {
        Console.WriteLine("No teacheers available.");
    }

    Console.WriteLine("List of teachers: ");
    foreach (var teacher in teachers)
    {
        Console.WriteLine($"- {teacher.Name}  (Username: {teacher.User.Username})");
    }
}
static void EditTeacher(SchoolDbContext context)
{
    ListTeachers(context);

    Console.WriteLine("Enter the current username of the teacher to edit (e.g., teacher_john): ");
    string currentUsername = Console.ReadLine()?.Trim();
    
    var user = context.Users.FirstOrDefault(u=> u.Username == currentUsername);

    if (user == null)
    {
        Console.WriteLine("User not found.");
        return;
    }

    var teacher = context.Teachers.FirstOrDefault(t=> t.UserId == user.Id);

    if (teacher == null) 
    {
        Console.WriteLine("Teacher not found.");
        return;
    }

    Console.WriteLine("New Teacher Name: ");
    string newName = Console.ReadLine()?.Trim();

    Console.WriteLine("New username (without 'teacher_'): ");
    string newUsername = Console.ReadLine()?.Trim();
    newUsername = "teacher_" + newUsername.ToLower();

    Console.WriteLine("New Password: ");
    string newPassword = Console.ReadLine()?.Trim();

    if (context.Users.Any(u => u.Username == newUsername && u.Id != user.Id));
    {
        Console.WriteLine("Username already taken by another user.");
        return;
    }

    teacher.Name = newName;
    user.Username = newUsername;
    user.Password = newPassword;

    context.SaveChanges();
    Console.WriteLine("Teacher updated successfully.");


} 
static void DeleteTeacher(SchoolDbContext context)
{
    ListTeachers(context);

    Console.Write("Enter the username of the teacher to delete (e.g., teacher_john): ");
    string username = Console.ReadLine()?.Trim();

    var user = context.Users.FirstOrDefault(u=> u.Username ==  username);
    if (user == null)
    {
        Console.WriteLine("User not found.");
        return;
    }

    var teacher = context.Teachers.FirstOrDefault(t=> t.UserId == user.Id);
    if (teacher == null)
    {
        Console.WriteLine("Teacher not found.");
        return;
    }

    context.Teachers.Remove(teacher);
    context.Users.Remove(user);
    context.SaveChanges();

    Console.WriteLine("Teacher deleted successfully.");
}
static void TeacherSubMenu(SchoolDbContext context)
{
    while (true)
    {
        Console.WriteLine("\nWhat would you like to do?");
        Console.WriteLine("1) Edit teacher");
        Console.WriteLine("2) Delete teacher");
        Console.WriteLine("3) Assign teacher");
        Console.WriteLine("0) Return to main menu");

        Console.Write("Enter your choice: ");
        string input = Console.ReadLine();

        switch (input)
        {
            case "1":
                EditTeacher(context);
                break;
            case "2":
                DeleteTeacher(context);
                break;
            case "3":
                AssignTeacherToSubject(context);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }
    }
}


#endregion

#region Class
static void CreateClass (SchoolDbContext context)
{
    Console.Write("Enter Class: ");
    string name = Console.ReadLine();

    var existing = context.Classes.FirstOrDefault(c=> c.Name.ToLower() == name.ToLower());

    if (existing != null)
    {
        Console.WriteLine("Class already exists.");
        return;
    }

    context.Classes.Add(new ClassRoom { Name = name });
    context.SaveChanges();
    Console.WriteLine("Class created successfully");

    ClassSubMenu(context);
}
static void ClassSubMenu(SchoolDbContext context)
{
    while (true)
    { 
        Console.WriteLine("\nWhat would you like to do?");
        Console.WriteLine("1) Edit class");
        Console.WriteLine("2) Delete class");
        Console.WriteLine("3) Assign subject to class");
        Console.WriteLine("0) Return to main menu");

        Console.Write("Enter your choice: ");
        string subChoice = Console.ReadLine();

        switch (subChoice)
        {
            case "1":
                EditClass(context);
                break;
            case "2":
                DeleteClass(context);
                break;
            case "3":
                AssignSubjectToClass(context);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid choice. Returning to main menu.");
                break;
        }

    }
}
static void EditClass (SchoolDbContext context)
{
    var classes = context.Classes.ToList();
    if (!classes.Any())
    {
        Console.WriteLine("No classes available to edite.");
        return;
    }

    ListClasses(context);

    Console.WriteLine("Provide information to edit class:");
    Console.Write("Current class name: ");
    string currentName = Console.ReadLine().Trim();

    var classRoom = context.Classes.FirstOrDefault(x => x.Name.ToLower() == currentName.ToLower());

    if (classRoom == null)
    {
        Console.WriteLine("Class not found\n");
        return;
    }

    Console.Write("New class name: ");
    string newName = Console.ReadLine().Trim();

    if (string.IsNullOrWhiteSpace(newName))
    {
        Console.WriteLine("Class name cannot be empty.");
        return;
    }
    classRoom.Name = newName;
    context.SaveChanges();
    Console.WriteLine("Class updated successfully");
}
static void DeleteClass(SchoolDbContext context)
{
    var classes = context.Classes.ToList();

    if (!classes.Any())
    {
        Console.WriteLine("No classes available to Delete.");
        return;
    }

     
    ListClasses(context);
    Console.WriteLine("Provide information to delete a class:");
    Console.Write("Class name: ");
    string className = Console.ReadLine();

    var classRoom = context.Classes.FirstOrDefault(x => x.Name == className);

    if (classRoom == null)
    {
        Console.WriteLine("Class not found");
        return;
    }
    context.Classes.Remove(classRoom);
    context.SaveChanges();
    Console.WriteLine("Class deleted successfully");

}
static void ListClasses (SchoolDbContext context)
{
    var classes = context.Classes.ToList();
    if (!classes.Any())
    {
        Console.WriteLine("No classes are present in the system.\n");
        return;
    }
    Console.WriteLine("\nFollowing classes are present in the system:");
    foreach (var c in classes) 
    {
        Console.WriteLine(c.Name);
    }

 
}
#endregion

#region Student
static void AddStudent(SchoolDbContext context)
{
    var classes = context.Classes.ToList();
    if (!classes.Any()) 
    {
        Console.WriteLine("No classes found, ask admin to create classes first.");
        return;
    }

    Console.WriteLine("\nPlease provide following information to add student: ");
    Console.Write("Class name: ");
    string className = Console.ReadLine();

    var selectedClass = context.Classes.FirstOrDefault(x => x.Name == className);

    if (selectedClass == null)
    {
        Console.WriteLine("Class not found.");
        return;
    }

    Console.Write("Student name: ");
    string studentName = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(studentName))
    {
        Console.WriteLine("Student name connot be empty");
    }

    var student = new Student
    {
        Name = studentName,
        ClassRoomId = selectedClass.Id
    };

    context.Students.Add(student);
    context.SaveChanges();
    Console.WriteLine($"Student {studentName} add {selectedClass.Name} successfully.");

    while (true)
    {
        Console.WriteLine("\nWhat would you like to do:");
        Console.WriteLine("1) Edit students");
        Console.WriteLine("2) Delete students");
        Console.WriteLine("3) View students");
        Console.WriteLine("0) Return to main menu");

        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                EditStudent(context);
                break;
            case "2":
                DeleteStudent(context);
                break;
            case "3":
                ViewStudents(context);
                break;
            case "0":
                return; 
            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }
}
static void EditStudent(SchoolDbContext context)
{
    var students = context.Students.Include(s => s.ClassRoom).ToList();
    if (!students.Any())
    {
        Console.WriteLine("No students found.");
        return;
    }

    Console.WriteLine("\nList of students:");
    foreach (var student in students)
    {
        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Class: {student.ClassRoom?.Name}");
    }

    Console.Write("\nEnter Student ID to edit: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var studentToEdit = context.Students.FirstOrDefault(s => s.Id == id);
    if (studentToEdit == null)
    {
        Console.WriteLine("Student not found.");
        return;
    }

    Console.Write("Enter new name: ");
    var newName = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(newName))
    {
        Console.WriteLine("Student name cannot be empty.");
        return;
    }

    studentToEdit.Name = newName;
    context.SaveChanges();

    Console.WriteLine("Student updated successfully.");
}
static void DeleteStudent(SchoolDbContext context)
{
    var students = context.Students.Include(s => s.ClassRoom).ToList();
    if (!students.Any())
    {
        Console.WriteLine("No students found.");
        return;
    }

    Console.WriteLine("\nList of students:");
    foreach (var student in students)
    {
        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Class: {student.ClassRoom?.Name}");
    }

    Console.Write("\nEnter Student ID to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var studentToDelete = context.Students.FirstOrDefault(s => s.Id == id);
    if (studentToDelete == null)
    {
        Console.WriteLine("Student not found.");
        return;
    }

    context.Students.Remove(studentToDelete);
    context.SaveChanges();

    Console.WriteLine("Student deleted successfully.");
}
static void ViewStudents(SchoolDbContext context)
{
    var students = context.Students.Include(s => s.ClassRoom).ToList();

    if (!students.Any())
    {
        Console.WriteLine("No students found.");
        return;
    }

    Console.WriteLine("\nList of students:");
    foreach (var student in students)
    {
        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Class: {student.ClassRoom?.Name}");
    }
}

#endregion`

#region Subject

static void CreateSubject (SchoolDbContext context)
{
    Console.WriteLine("Enter Subject Name: ");
    string name = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Subject name cannot be empty.");
        return;
    }

    context.Subjects.Add(new Subject { Name = name });
    context.SaveChanges();
    Console.WriteLine("Subject Created Successfully.");

}
static void EditSubject(SchoolDbContext context)
{
    ListSubjects(context);

    Console.WriteLine("Provide following information to edit a subject:");
    Console.Write("Current subject name: ");
    string currentName = Console.ReadLine();

    var subject = context.Subjects.FirstOrDefault(x => x.Name == currentName);

    if (subject == null) 
    {
        Console.WriteLine("Subject not found.");
        return;
    }

    Console.Write("New subject name: ");
    string newName = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(newName))
    {
        Console.WriteLine("Subject name cannot be empty.");
        return;
    }

    subject.Name = newName;
    context.SaveChanges();

    Console.WriteLine("Subject updated successfully.");
}
static void DeleteSubject(SchoolDbContext context)
{
    ListSubjects(context);

    Console.WriteLine("Provide following information to delete a subject:");
    Console.Write("Subject name: ");
    string subjectName = Console.ReadLine();

    var subject = context.Subjects.FirstOrDefault(s => s.Name == subjectName);
    if (subject == null)
    {
        Console.WriteLine("Subject not found.");
        return;
    }

    context.Subjects.Remove(subject);
    context.SaveChanges();

    Console.WriteLine("Subject deleted successfully.");
}
static void ListSubjects(SchoolDbContext context)
{
    var subjects = context.Subjects.ToList();

    if (!subjects.Any()) 
    {
        Console.WriteLine("No subjects are present in the system.\n");
        return;
    }

    Console.WriteLine("Following subjects are present in the system:");
    foreach (var subject in subjects)
    {
        Console.WriteLine(subject.Name);
    }
}
static void SubjectSubMenu(SchoolDbContext context)
{
    while (true)
    {
        Console.WriteLine("\nWhat would you like to do?");
        Console.WriteLine("1) Edit subject");
        Console.WriteLine("2) Delete subject");
        Console.WriteLine("3) Assign subject to class");
        Console.WriteLine("0) Return to main menu");

        Console.Write("Enter your choice: ");
        string input = Console.ReadLine();

        switch (input)
        {
            case "1":
                EditSubject(context);
                break;
            case "2":
                DeleteSubject(context);
                break;
            case "3":
                AssignSubjectToClass(context);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }
    }
}


#endregion

#region Grades
static void InsertGrades(SchoolDbContext context, int teahcerId)
{
    Console.WriteLine("Please provide following information to insert grades: ");

    Console.Write("Class name: ");
    string className = Console.ReadLine()?.Trim().ToLower();

    Console.Write("Subject name: ");
    string subjectName = Console.ReadLine()?.Trim().ToLower();

    Console.Write("Student name: ");
    string studentName = Console.ReadLine()?.Trim().ToLower();

    Console.Write("Term name (1st, mid, final): ");
    string termInput = Console.ReadLine()?.Trim().ToLower();

    Console.Write("Grade (0.00 to 5.00): ");
    string gradeInput = Console.ReadLine()?.Trim();

    var classRoom = context.Classes.FirstOrDefault(c => c.Name.ToLower() == className);
    if (classRoom == null)
    {
        Console.WriteLine("Class not found.");
        return;
    }
    
    var subject = context.Subjects.FirstOrDefault(s=> s.Name.ToLower() == subjectName);
    if (subject == null)
    {
        Console.WriteLine("Subject not found.");
        return;
    }

    var student = context.Students.FirstOrDefault(s=> 
        s.Name.ToLower() == studentName && 
        s.ClassRoomId == classRoom.Id );
    if (student == null)
    {
        Console.WriteLine("Student not found in the class");
        return;
    }

    Term term;

    switch (termInput)
    {
        case "1st":
            term = Term.First; 
            break;
        case "mid":
            term = Term.Mid;
            break;
        case "final":
            term = Term.Final;
            break;
        default:
            Console.WriteLine("Invalid term. Use '1st', 'mid', or 'final'.");
            return;
    }

    if (!double.TryParse(gradeInput, out double gradeValue) || gradeValue < 0.0 || gradeValue > 5.0)
    {
        Console.WriteLine("Invalid grade. Grade must be between 0.00 and 5.00.");
        return;
    }

    var assignment = context.Assignments.FirstOrDefault(a =>
        a.ClassRoomId == classRoom.Id &&
        a.SubjectId == subject.Id);

    if (assignment == null)
    {
        Console.WriteLine("Assignment Class & Subject not found.");
        return;
    }

    var existingGrades = context.Grades.FirstOrDefault(g =>
        g.StudentId == student.Id &&
        g.AssignmentId == assignment.Id &&
        g.Term == term);

    if (existingGrades != null) 
    {
        existingGrades.GradeValue = gradeValue;
        Console.WriteLine("Grade updated successfully.");
    }
    else
    {
        context.Grades.Add(new Grade
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            Term = term,
            GradeValue = gradeValue
        });
        Console.WriteLine("Grade inserted successfully.");
    }
    context.SaveChanges();
}
static void ViewGrades(SchoolDbContext context, int teacherId)
{
    var assignments = context.Assignments
        .Include(x=> x.Subject)
        .Include(x=> x.ClassRoom)
        .Where(x=> x.TeacherId == teacherId)
        .ToList();

    if (!assignments.Any())
    {
        Console.WriteLine("No assignment found");
        return;
    }

    Console.WriteLine("\nGrades by sujcect: ");
    for (int i = 0; i < assignments.Count; i++) 
    {
        Console.WriteLine($"{i+1}) {assignments[i].Subject.Name} -> {assignments[i].ClassRoom.Name}");
    }

    Console.Write("\nSelect assignment number to view grades: ");
    if (!int.TryParse(Console.ReadLine(), out int assignmentChoice) || 
        assignmentChoice < 1 || assignmentChoice > assignments.Count)
    {
        Console.WriteLine("Invalid choice");
        return;
    }

    var selectedAssignment = assignments[assignmentChoice - 1];

    Console.WriteLine("Select Term: ");
    Console.WriteLine("1) 1st");
    Console.WriteLine("2) Mid ");
    Console.WriteLine("3) Final");
    Console.Write("Enter your choice: ");

    if (!int.TryParse(Console.ReadLine(), out int termInput) || termInput < 1 || termInput > 3)
    {
        Console.WriteLine("Invalid term selected");
        return;
    }

    Term selectedTerm = (Term)termInput;

    var students = context.Students.Where(x=> x.ClassRoomId == selectedAssignment.ClassRoomId).ToList();

    if (!students.Any())
    {
        Console.WriteLine("No students found in this class.");
        return;
    }

    Console.WriteLine($"\nGrades for {selectedAssignment.Subject.Name} - {selectedAssignment.ClassRoom.Name} - Term: {GetTermLabel(selectedTerm)}");

    foreach (var student in students)
    {
        var grade = context.Grades.FirstOrDefault(g=>
        g.AssignmentId == selectedAssignment.Id &&
        g.StudentId == student.Id &&
        g.Term == selectedTerm);

        string gradeText = grade != null ? grade.GradeValue.ToString() : "No grade available.";
        Console.WriteLine($"Student: {student.Name}, Grade: {gradeText}");
    }
}
static void ShowAllGradesByClass(SchoolDbContext context)
{
    Console.Write("Enter class name: ");
    string className = Console.ReadLine()?.Trim().ToLower();

    var classRoom = context.Classes
       .FirstOrDefault(c => c.Name.ToLower() == className);
    
    if (classRoom == null)
    {
        Console.WriteLine("Class not found.");
        return;
    }

    var students = context.Students
        .Where(s => s.ClassRoomId == classRoom.Id).ToList();

    var assignments = context.Assignments.
        Where(a=> a.ClassRoomId == classRoom.Id).ToList() ;

    Console.WriteLine($"\nShowign grades for {classRoom.Name}:" );
    Console.WriteLine($"{"Name",-20} {"1st",5} {"Mid",5} {"Final",7}");
    
    foreach (var student in students) 
    {
        double? first = null, mid = null, final = null;

        foreach (var assignment in assignments)
        {
            var grades = context.Grades
                .Where(g => g.StudentId == student.Id && g.AssignmentId == assignment.Id);

            foreach ( var grade in grades)
            {
                switch (grade.Term)
                {
                    case Term.First:
                        first = first == null ? grade.GradeValue : (first + grade.GradeValue) / 2;
                        break;
                    case Term.Mid:
                        mid = mid == null ? grade.GradeValue : (mid + grade.GradeValue) / 2;
                        break;
                    case Term.Final:
                        final = final == null ? grade.GradeValue : (final + grade.GradeValue) / 2;
                        break;
                }
            }
        }
        string f = first?.ToString("0.0") ?? "-";
        string m = mid?.ToString("0.0") ?? "-";
        string fi = final?.ToString("0.0") ?? "-";

        Console.WriteLine($"{student.Name,-20} {f,5} {m,5} {fi,7}");
    }
    Console.WriteLine();

}
static string GetTermLabel(Term term)
{
    return term switch
    {
        Term.First => "1st",
        Term.Mid => "Mid",
        Term.Final => "Final",
        _ => term.ToString()
    };
}

#endregion

#region Assignning

static void AssignSubjectToClass(SchoolDbContext context)
{
    ListClasses(context);
    Console.WriteLine("Provide information to assign a subject to class: ");
    Console.Write("Class name: ");
    string className = Console.ReadLine();

    var classRoom = context.Classes.FirstOrDefault(x => x.Name == className);

    if (classRoom == null)
    {
        Console.WriteLine("Class not found.");
        return;
    }

    ListSubjects(context);
    Console.Write("Subject name: ");
    string subjectName = Console.ReadLine();

    var subject = context.Subjects.FirstOrDefault(x => x.Name == subjectName);

    if (subject == null)
    {
        Console.WriteLine("Subject not found.");
        return;
    }

    bool alreadyAssigned = context.Assignments
        .Any(x => x.ClassRoomId == classRoom.Id &&
        x.SubjectId == subject.Id);
    if (alreadyAssigned)
    {
        Console.WriteLine("This subject is already assigned to this class.");
        return;
    }

    context.Assignments.Add(new Assignment
    {
        ClassRoomId = classRoom.Id,
        SubjectId = subject.Id

    });

    context.SaveChanges();
    Console.WriteLine($"{subject.Name} assigned to {classRoom.Name} successfully.");
}
static void AssignTeacherToSubject(SchoolDbContext context)
{
    Console.WriteLine("Provide following information to assign a teacher:");
    Console.Write("Class name: ");
    string className = Console.ReadLine()?.Trim().ToLower(); 

    Console.Write("Subject name: ");
    string subjectName = Console.ReadLine()?.Trim().ToLower();

    Console.Write("Teacher name (e.g. Saifur): ");
    string teacherName = Console.ReadLine()?.Trim().ToLower();

    var classRoom = context.Classes.FirstOrDefault(x => x.Name.ToLower() == className);
    var subject = context.Subjects.FirstOrDefault(x => x.Name.ToLower() == subjectName);
    var teacher = context.Teachers.FirstOrDefault(x => x.Name.ToLower()== teacherName);

    if(classRoom == null) Console.WriteLine("Class not found.");
    if(subject == null) Console.WriteLine("Subject not found.");
    if (teacher == null) Console.WriteLine("teacher not found.");

    if (classRoom == null || subject == null || teacher == null)
    {
        Console.WriteLine("Invalid class, subject, or teacher name.");
        return;
    }

    var assignment = context.Assignments.FirstOrDefault(x =>
        x.ClassRoomId == classRoom.Id &&
        x.SubjectId == subject.Id);

    if (assignment != null)
    {
        assignment.TeacherId = teacher.Id;
        Console.WriteLine($"Updated teacher to '{teacher.Name}' for {subject.Name} in {classRoom.Name}.");
    }
    else
    {
        context.Assignments.Add(new Assignment 
        {
            ClassRoomId = classRoom.Id,
            SubjectId = subject.Id,
            TeacherId = teacher.Id
        });
        Console.WriteLine($"Assigned teacher '{teacher.Name}' to {subject.Name} in {classRoom.Name}.");
    }
    context.SaveChanges();
}
static void ViewAssignments(SchoolDbContext context)
{
    var assignments = context.Assignments
        .Include(x => x.ClassRoom)
        .Include(x => x.Subject)
        .Include(x => x.Teacher)
        .ToList();

    if (!assignments.Any())
    {
        Console.WriteLine("No assignments found");
        return;
    }
    Console.WriteLine("\nClass-Subject-Teacher Mapping: ");
    foreach (var assignment in assignments)
    {
        string teacherName = assignment.Teacher?.Name ?? "No teacher assigned";
        Console.WriteLine($"Class: {assignment.ClassRoom.Name}, Subject: {assignment.Subject.Name}, Teacher: {teacherName}");
    }
    Console.WriteLine();
}

#endregion



