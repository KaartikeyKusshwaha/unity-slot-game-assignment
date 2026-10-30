"""Build the game's SFX from the included CC0 Kenney source recordings.
Requires numpy and soundfile. Run from the repository root.
"""
from pathlib import Path
import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parents[1]
SOUNDS = ROOT / "Assets/Sounds"
RATE = 44100

def sample(pack, name):
    data, rate = sf.read(SOUNDS / "Kenney" / pack / (name + ".ogg"), always_2d=True)
    data = data.mean(axis=1)
    if rate != RATE:
        data = np.interp(np.arange(int(len(data)*RATE/rate))*rate/RATE, np.arange(len(data)), data)
    active = np.flatnonzero(np.abs(data) > .0001)
    if len(active):
        data = data[max(0, active[0]-88):min(len(data), active[-1]+662)]
    return data

def pitch(data, ratio):
    return np.interp(np.arange(0,len(data),ratio),np.arange(len(data)),data)

def mix(seconds, layers):
    output = np.zeros(int(seconds*RATE))
    for start, data, gain in layers:
        begin = int(start*RATE)
        length = min(len(data),len(output)-begin)
        if length > 0:
            output[begin:begin+length] += data[:length]*gain
    return output

def save(name, data, peak=.75):
    maximum = np.max(np.abs(data))
    if maximum > 0: data = data / maximum * peak
    fade = min(440,len(data)//4)
    data[-fade:] *= np.linspace(1,0,fade)
    sf.write(SOUNDS/(name+".wav"), data, RATE, subtype="PCM_16")

casino = lambda name: sample("casino-audio",name)
ui = lambda name: sample("interface-sounds",name)
jingle = lambda name: sample("music-jingles",name)

save("click",ui("click_002"),.55)
save("entry",jingle("jingles_PIZZI00"),.7)
save("lever",mix(.45,[(0,ui("switch_002"),.7),(.035,casino("chips-handle-3"),.7)]),.7)
save("latch",casino("chips-stack-4"),.65)
save("tick",casino("chip-lay-1")[:int(.055*RATE)],.55)
save("count",pitch(ui("pluck_001"),1.35),.55)
save("stop",mix(.34,[(0,casino("chip-lay-3"),.9),(.012,ui("glass_001"),.6),(.008,ui("pluck_001"),.4)]),.8)
save("spin",mix(.8,[(i*.12,casino("chips-collide-1"),.8-i*.08) for i in range(5)]),.55)
loop = mix(1,[(0,ui("scroll_001"),.5)]+[(i*.1,pitch(casino("chip-lay-2"),.7),.12) for i in range(10)])
# Crossfade the loop boundary to avoid a hard click.
edge=2205
loop[:edge]=loop[:edge]*np.linspace(0,1,edge)+loop[-edge:]*np.linspace(1,0,edge)
save("rumble",loop,.6)
save("win",mix(1.7,[(0,jingle("jingles_PIZZI09"),.9),(.55,jingle("jingles_PIZZI10"),.9),(.8,casino("chips-collide-2"),.35)]),.8)
save("collect",mix(.85,[(i*.09,pitch(casino("chips-collide-1" if i%2 else "chips-collide-2"),.94+i*.02),.7) for i in range(7)]+[(.65,ui("glass_001"),.3)]),.8)
print("Built 11 mixed SFX cues from CC0 source recordings.")
