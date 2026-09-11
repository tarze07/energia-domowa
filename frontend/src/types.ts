export type Room = {
  id: number
  name: string
  deviceCount: number
}

export type Device = {
  id: number
  name: string
  roomId: number
  roomName: string
  powerWatts: number
  hoursPerDay: number
  isActive: boolean
  dailyKwh: number
  monthlyKwh: number
  monthlyCost: number
}

export type MeterReading = {
  id: number
  date: string
  totalKwh: number
  note: string | null
  deltaKwh: number | null
}

export type DailyConsumption = {
  id: number
  date: string
  kwh: number
  cost: number
}

export type Tariff = {
  id: number
  name: string
  pricePerKwh: number
  dailyLimitKwh: number
  monthlyBudgetPln: number
}

export type ChartPoint = {
  date: string
  kwh: number
  cost: number
}

export type TopDevice = {
  name: string
  roomName: string
  dailyKwh: number
  monthlyKwh: number
  monthlyCost: number
}

export type Dashboard = {
  todayKwh: number
  todayCost: number
  monthKwh: number
  monthCost: number
  prevMonthKwh: number
  deviceEstimateDailyKwh: number
  deviceEstimateMonthlyKwh: number
  deviceEstimateMonthlyCost: number
  dailyLimitKwh: number
  monthlyBudgetPln: number
  budgetUsedPercent: number
  overDailyLimit: boolean
  alerts: string[]
  last30Days: ChartPoint[]
  topDevices: TopDevice[]
}

export type Page = 'pulpit' | 'urzadzenia' | 'zuzycie' | 'taryfa'
