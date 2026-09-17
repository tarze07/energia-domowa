import { useEffect, useState } from 'react'
import { api } from '../api'
import { BarChart } from '../components/BarChart'
import { fmtDate, fmtKwh, fmtPln, todayIso } from '../format'
import type { Dashboard } from '../types'

function forecastHint(data: Dashboard, today: string): string {
  if (data.budgetExhaustionDate) {
    const when = fmtDate(data.budgetExhaustionDate)
    if (data.budgetExhaustionDate <= today) {
      return `Budżet wyczerpany od ${when}.`
    }
    return `Przy tym tempie budżet skończy się ${when}.`
  }
  return `Prognoza w budżecie ${fmtPln(data.monthlyBudgetPln)}.`
}

export function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .dashboard()
      .then(setData)
      .catch((err: unknown) => setError(err instanceof Error ? err.message : 'Błąd'))
  }, [])

  if (error) {
    return <p className="error">{error}</p>
  }
  if (!data) {
    return <p className="muted">Ładowanie pulpitu…</p>
  }

  const monthDelta = data.prevMonthKwh === 0 ? 0 : ((data.monthKwh - data.prevMonthKwh) / data.prevMonthKwh) * 100

  return (
    <div className="stack">
      <header className="page-head">
        <div>
          <p className="eyebrow">Domowy monitoring</p>
          <h1>Zużycie energii</h1>
        </div>
      </header>

      {data.alerts.length > 0 && (
        <section className="alerts">
          {data.alerts.map((alert) => (
            <p key={alert}>{alert}</p>
          ))}
        </section>
      )}

      <section className="kpis">
        <article className="kpi">
          <span>Dziś</span>
          <strong>{fmtKwh(data.todayKwh)}</strong>
          <small>{fmtPln(data.todayCost)}</small>
        </article>
        <article className="kpi">
          <span>Ten miesiąc</span>
          <strong>{fmtKwh(data.monthKwh)}</strong>
          <small>
            {fmtPln(data.monthCost)} · {monthDelta >= 0 ? '+' : ''}
            {monthDelta.toFixed(0)}% vs poprz.
          </small>
        </article>
        <article className="kpi">
          <span>Szacunek urządzeń</span>
          <strong>{fmtKwh(data.deviceEstimateDailyKwh)}/d</strong>
          <small>{fmtPln(data.deviceEstimateMonthlyCost)} / 30 dni</small>
        </article>
        <article className="kpi">
          <span>Budżet</span>
          <strong>{data.budgetUsedPercent.toFixed(0)}%</strong>
          <small>
            limit {fmtKwh(data.dailyLimitKwh)}/d · {fmtPln(data.monthlyBudgetPln)}
          </small>
        </article>
      </section>

      <section className="panel">
        <div className="panel-head">
          <h2>Prognoza końca miesiąca</h2>
          <small>{data.daysLeftInMonth} dni zostało</small>
        </div>
        {data.forecastMonthKwh === 0 ? (
          <p className="muted">Brak danych do prognozy.</p>
        ) : (
          <>
            <p className="forecast-num">
              <strong>{fmtKwh(data.forecastMonthKwh)}</strong>
              <span>{fmtPln(data.forecastMonthCost)}</span>
            </p>
            {data.monthlyBudgetPln > 0 && (
              <>
                <p className="muted">
                  {forecastHint(data, todayIso())}
                </p>
                <div className="meter">
                  <div
                    className={data.forecastMonthCost > data.monthlyBudgetPln ? 'meter-fill over' : 'meter-fill'}
                    style={{
                      width: `${Math.min(100, (data.monthCost / data.monthlyBudgetPln) * 100)}%`,
                    }}
                  />
                </div>
              </>
            )}
          </>
        )}
      </section>

      <section className="panel">
        <div className="panel-head">
          <h2>Ostatnie 30 dni</h2>
          <small>Linia = dzienny limit</small>
        </div>
        <BarChart points={data.last30Days} limit={data.dailyLimitKwh} />
      </section>

      <section className="panel">
        <div className="panel-head">
          <h2>Zużycie wg pomieszczeń</h2>
          <small>szacunek urządzeń</small>
        </div>
        {data.rooms.length === 0 ? (
          <p className="muted">Brak aktywnych urządzeń.</p>
        ) : (
          <div className="room-list">
            {data.rooms.map((room) => (
              <div key={room.name} className="room-row">
                <div className="row">
                  <strong>{room.name}</strong>
                  <small>
                    {fmtKwh(room.dailyKwh)}/d · {fmtPln(room.monthlyCost)} · {room.sharePercent}%
                  </small>
                </div>
                <div className="meter">
                  <div className="meter-fill" style={{ width: `${room.sharePercent}%` }} />
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="panel">
        <div className="panel-head">
          <h2>Największe odbiorniki</h2>
        </div>
        <table>
          <thead>
            <tr>
              <th>Urządzenie</th>
              <th>Pomieszczenie</th>
              <th>Dzień</th>
              <th>Miesiąc</th>
              <th>Koszt</th>
            </tr>
          </thead>
          <tbody>
            {data.topDevices.map((device) => (
              <tr key={`${device.roomName}-${device.name}`}>
                <td>{device.name}</td>
                <td>{device.roomName}</td>
                <td>{fmtKwh(device.dailyKwh)}</td>
                <td>{fmtKwh(device.monthlyKwh)}</td>
                <td>{fmtPln(device.monthlyCost)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  )
}
