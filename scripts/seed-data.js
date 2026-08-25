const employees = db.getCollection('employees')
const projects = db.getCollection('projects')
const timeEntries = db.getCollection('time_entries')
const closedPeriods = db.getCollection('closed_periods')

timeEntries.deleteMany({})
closedPeriods.deleteMany({})
employees.deleteMany({})
projects.deleteMany({})

const ivanovId = ObjectId('65a000000000000000000001')
const petrovaId = ObjectId('65a000000000000000000002')
const project001Id = ObjectId('65b000000000000000000001')
const project002Id = ObjectId('65b000000000000000000002')

employees.insertMany([
  {
    _id: ivanovId,
    fullName: 'Иванов И. А.',
    department: 'Проектный',
    rates: [
      { from: ISODate('2026-01-01T00:00:00.000Z'), value: Decimal128('500') },
      { from: ISODate('2026-03-01T00:00:00.000Z'), value: Decimal128('600') },
    ],
  },
  {
    _id: petrovaId,
    fullName: 'Петрова И. С.',
    department: 'Проектный',
    rates: [
      { from: ISODate('2026-02-01T00:00:00.000Z'), value: Decimal128('700') },
    ],
  },
])

projects.insertMany([
  {
    _id: project001Id,
    code: 'П-001',
    name: 'Реконструкция цеха',
    budget: Decimal128('20000'),
    startDate: ISODate('2026-01-01T00:00:00.000Z'),
    endDate: ISODate('2026-03-31T00:00:00.000Z'),
  },
  {
    _id: project002Id,
    code: 'П-002',
    name: 'Инженерные сети',
    budget: Decimal128('5000'),
    startDate: ISODate('2026-03-01T00:00:00.000Z'),
    endDate: null,
  },
])

const createdAt = ISODate('2026-01-01T00:00:00.000Z')

timeEntries.insertMany([
  {
    _id: ObjectId('65c000000000000000000001'),
    employeeId: ivanovId,
    projectId: project001Id,
    date: ISODate('2026-02-20T00:00:00.000Z'),
    hours: Decimal128('8'),
    comment: 'Февральские работы',
    version: NumberLong(1),
    createdAtUtc: createdAt,
    updatedAtUtc: createdAt,
  },
  {
    _id: ObjectId('65c000000000000000000002'),
    employeeId: ivanovId,
    projectId: project001Id,
    date: ISODate('2026-03-05T00:00:00.000Z'),
    hours: Decimal128('8'),
    comment: 'Работы по реконструкции',
    version: NumberLong(1),
    createdAtUtc: createdAt,
    updatedAtUtc: createdAt,
  },
  {
    _id: ObjectId('65c000000000000000000003'),
    employeeId: petrovaId,
    projectId: project001Id,
    date: ISODate('2026-03-05T00:00:00.000Z'),
    hours: Decimal128('4'),
    comment: 'Проектные работы',
    version: NumberLong(1),
    createdAtUtc: createdAt,
    updatedAtUtc: createdAt,
  },
  {
    _id: ObjectId('65c000000000000000000004'),
    employeeId: petrovaId,
    projectId: project002Id,
    date: ISODate('2026-03-06T00:00:00.000Z'),
    hours: Decimal128('10'),
    comment: 'Инженерные сети',
    version: NumberLong(1),
    createdAtUtc: createdAt,
    updatedAtUtc: createdAt,
  },
])

print('Seed completed.')
print(`employees: ${employees.countDocuments({})}`)
print(`projects: ${projects.countDocuments({})}`)
print(`time_entries: ${timeEntries.countDocuments({})}`)
print(`closed_periods: ${closedPeriods.countDocuments({})}`)
