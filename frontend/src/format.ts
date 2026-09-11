const pl = new Intl.NumberFormat('pl-PL', { maximumFractionDigits: 2 })
const pl1 = new Intl.NumberFormat('pl-PL', { minimumFractionDigits: 1, maximumFractionDigits: 1 })
const money = new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' })

export function fmtKwh(value: number): string {
  return `${pl.format(value)} kWh`
}

export function fmtKwh1(value: number): string {
  return `${pl1.format(value)} kWh`
}

export function fmtPln(value: number): string {
  return money.format(value)
}

export function fmtDate(iso: string): string {
  const [y, m, d] = iso.split('-')
  return `${d}.${m}.${y}`
}

export function todayIso(): string {
  const now = new Date()
  const y = now.getFullYear()
  const m = String(now.getMonth() + 1).padStart(2, '0')
  const d = String(now.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}
