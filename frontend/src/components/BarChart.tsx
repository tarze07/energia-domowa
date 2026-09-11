import type { ChartPoint } from '../types'
import { fmtDate } from '../format'

type Props = {
  points: ChartPoint[]
  limit: number
}

export function BarChart({ points, limit }: Props) {
  if (points.length === 0) {
    return <p className="muted">Brak danych do wykresu.</p>
  }

  const max = Math.max(limit, ...points.map((p) => p.kwh), 1)
  const w = 720
  const h = 220
  const pad = 28
  const innerW = w - pad * 2
  const innerH = h - pad * 2
  const gap = 3
  const barW = Math.max(4, innerW / points.length - gap)
  const limitY = pad + innerH - (limit / max) * innerH

  return (
    <svg className="chart" viewBox={`0 0 ${w} ${h}`} role="img" aria-label="Zużycie z 30 dni">
      <line
        x1={pad}
        x2={w - pad}
        y1={limitY}
        y2={limitY}
        className="chart-limit"
      />
      {points.map((point, index) => {
        const barH = (point.kwh / max) * innerH
        const x = pad + index * (barW + gap)
        const y = pad + innerH - barH
        const over = limit > 0 && point.kwh > limit
        return (
          <g key={point.date}>
            <rect
              x={x}
              y={y}
              width={barW}
              height={Math.max(barH, 1)}
              rx={2}
              className={over ? 'bar over' : 'bar'}
            >
              <title>
                {fmtDate(point.date)} — {point.kwh} kWh
              </title>
            </rect>
          </g>
        )
      })}
    </svg>
  )
}
